namespace Mock.Data.Generators
{
    using Microsoft.EntityFrameworkCore;
    using Mock.Data.Interfaces;
    using MockDataGenerator.EntityFramework.Core.Mock.Data.Types;
    using System.Text.Json;
    using OpenAI.Chat;
    using System.Text;
    using Repositories.Generic;

    public class MockDataGenerator<T> where T : DbContext
    {
        internal T _context;
        internal List<dynamic> _generatedEntities;
        internal ITraceWriter _trace;
        internal const string _openApiDefaultModelName = "gpt-4o";
        internal ChatClient _openAiChatClient;
        internal int _openAiBatchCount;

        public MockDataGenerator(T context, ITraceWriter trace, string openAiApiKey, string? openApiModelName = null)
        {
            _context = context;
            _trace = trace;
            _generatedEntities = new List<dynamic>();
            _openAiChatClient = new ChatClient(openApiModelName ?? _openApiDefaultModelName, openAiApiKey);
            _openAiBatchCount = 25;
        }

        public Entity AnalyseEntity<K>() where K : class
        {
            var entityTypeName = typeof(K).Name;
            var entityTypes = _context.Model.GetEntityTypes();
            var entityType = entityTypes.Where(et => et?.DisplayName() == entityTypeName).FirstOrDefault();
            //var startDateTime = DateTime.Now;
            //var startDateTimeString = startDateTime.ToString("dd/MM/yyyy HH:mm:ss");
            var displayName = entityType?.DisplayName();

            //_trace.Log($"Started analysing Model: {displayName}");

            var entity = new Entity();
            var properties = entityType?.GetProperties()?.ToList()!;

            entity.DisplayName = displayName;
            entity.Properties = properties;

            //var endDateTime = DateTime.Now;
            //var endDateTimeString = endDateTime.ToString("dd/MM/yyyy HH:mm:ss");
            //var timeElapsedString = (endDateTime - startDateTime).TotalMilliseconds;

            //_trace.Log($"{JsonSerializer.Serialize(_entities)}");
            //_trace.Log($"Finished analysing Model: {displayName}");
            //_trace.Log($"Operation took {timeElapsedString} milli seconds");

            return entity;
        }

        public void GenerateMockData<K>(int noOfRows, int primaryKeyStartIndexAt, string nullableForeignKeyDefaultClrTypeName) where K : class
        {
            int batchArrSize = noOfRows / _openAiBatchCount;
            int remainder = noOfRows % _openAiBatchCount;
            List<int> batchArr = new List<int>(remainder > 0 ? batchArrSize + 1 : batchArrSize);

            for (int i = 0; i < batchArrSize; i++)
            {
                batchArr.Add(_openAiBatchCount);
            }

            if (remainder > 0)
            {
                batchArr.Add(remainder);
            }

            Entity entity = AnalyseEntity<K>();
            var properties = entity.Properties!;
            //bool mockDataHasValue = false;

            foreach (var batchArrItem in batchArr)
            {
                int remainingCount = batchArrItem;

                while (remainingCount > 0)
                {
                    GenericRepository<T, K> genericRepository = new GenericRepository<T, K>(_context);
                    Type entityType = typeof(K);
                    string? displayName = entity?.DisplayName;
                    var entityProperties = new List<Property>();

                    StringBuilder sbMessage = new StringBuilder($"create dummy data in JSON format for {displayName} table with {remainingCount} rows where each {displayName} has a ");

                    foreach (var property in properties)
                    {
                        int propertiesCount = properties.Count();
                        int index = properties.IndexOf(property);
                        string? clrTypeName = property.ClrType.Name;
                        string typeName = (clrTypeName?.Equals("Nullable`1") ?? false) ? nullableForeignKeyDefaultClrTypeName : clrTypeName!;
                        bool messageAppended = false;

                        if (property.IsPrimaryKey() && property?.GetAfterSaveBehavior().ToString() != AfterSaveBehavior.Throw.ToString())
                        {
                            sbMessage.Append("PrimaryKey ");
                            sbMessage.Append($"{property!.Name} of type {typeName}");

                            if (property.ClrType.Name == "Int64" || property.ClrType.Name == "Int32")
                            {
                                sbMessage.Append($" start at index {primaryKeyStartIndexAt}");
                            }

                            messageAppended = true;
                        }
                        else if (!property.IsPrimaryKey() && property?.GetAfterSaveBehavior().ToString() != AfterSaveBehavior.Throw.ToString() && !property!.IsForeignKey())
                        {
                            sbMessage.Append($"{property.Name} of type {typeName}");
                            messageAppended = true;
                        }

                        // if (mockDataHasValue && property.IsPrimaryKey())
                        // {
                        //     var notInList = string.Join(", ", entity?.MockData?.Select(md => ((PropertyInfo[])md.GetType().GetProperties()).Where(p => p.Name == property.Name).FirstOrDefault()?.GetValue(md)?.ToString()).ToList()!);

                        //     sbMessage.Append($" not in list [{notInList}]");
                        // }

                        if (!(index == propertiesCount - 1) && messageAppended)
                        {
                            sbMessage.Append(", ");
                            messageAppended = false;
                        }
                    }

                    string message = sbMessage.ToString()!;

                    _trace.Log($"Sending [MESSAGE]: {message}");

                    ChatCompletion completion = _openAiChatClient.CompleteChat(message);
                    string completionContentText = completion.Content[0].Text;
                    int startIndex = completionContentText.IndexOf("```json") + 7;
                    int length = completionContentText.LastIndexOf("```") - startIndex;
                    StringBuilder sbCompletionContent = new StringBuilder(completionContentText.Substring(startIndex, length));
                    string completionContentString = sbCompletionContent.ToString();

                    _trace.Log($"[ASSISTANT]: {completionContentString}");

                    var deserializedMockData = JsonSerializer.Deserialize<List<K>>(completionContentString);
                    var typeCastedMockData = deserializedMockData?.Select((md) => (dynamic)md).ToList();

                    if (entity?.MockData?.Count() > 0)
                    {
                        entity?.MockData?.AddRange(typeCastedMockData!);
                    }
                    else
                    {
                        entity!.MockData = typeCastedMockData;
                        //mockDataHasValue = true;
                    }

                    _generatedEntities.Add(entity!);

                    var foreignKeyProperties = entity?.Properties?.Where(p => p.IsForeignKey()).ToList();

                    deserializedMockData?.ForEach((K data) =>
                    {
                        Type dataType = data.GetType();
                        var dataTypeVirtualProperties = dataType.GetProperties().Where((p) => p.GetMethod?.IsVirtual ?? false).ToList();

                        dataTypeVirtualProperties.ForEach((vp) =>
                        {
                            var principalEntityName = vp.DeclaringType?.Name;

                            if (principalEntityName != dataType.Name)
                            {
                                var principalEntity = _generatedEntities.Where(ge => ge?.DisplayName == principalEntityName).FirstOrDefault();
                                var mockData = (List<dynamic>?)principalEntity?.MockData;

                                var count = mockData?.Count ?? 0;

                                if (count > 0)
                                {
                                    Random random = new Random();
                                    int index = random.Next(0, count - 1);

                                    vp?.SetValue(data, mockData?[index]);
                                }
                            }
                        });

                        // foreignKeyProperties?.ForEach((property) =>
                        // {
                        //     if (property.IsForeignKey())
                        //     {
                        //         var principals = property.GetPrincipals().LastOrDefault()?.DeclaringType.Name.Split('.').ToList();

                        //         if (principals?.FirstOrDefault() != principals?.LastOrDefault())
                        //         {
                        //             var principalEntity = _generatedEntities.Where(ge => ge?.DisplayName == principals?.LastOrDefault()).FirstOrDefault();
                        //             var primaryKeyProperty = ((List<IProperty>?)principalEntity?.Properties)?.Where(p => p.IsPrimaryKey()).FirstOrDefault();
                        //             var mockData = (List<dynamic>?)principalEntity?.MockData;
                        //             var count = mockData?.Count ?? 0;

                        //             if (count > 0)
                        //             {
                        //                 Random random = new Random();
                        //                 int index = random.Next(0, count - 1);

                        //                 if ((AfterSaveBehavior?)primaryKeyProperty?.GetAfterSaveBehavior() == AfterSaveBehavior.Throw)
                        //                 {
                        //                     var dataTypeProperty = dataType.GetProperties().Where(p => p.Name == property.Name).FirstOrDefault();

                        //                     dataTypeProperty?.SetValue(data, index);
                        //                 }
                        //                 else
                        //                 {
                        //                     var principalPrimaryKeyList = mockData?.Select(md => ((PropertyInfo[])md.GetType().GetProperties()).Where(p => p.Name == primaryKeyProperty?.Name).FirstOrDefault()?.GetValue(md)).ToList()!;
                        //                     var dataTypeProperty = dataType.GetProperties().Where(p => p.Name == property.Name).FirstOrDefault();

                        //                     dataTypeProperty?.SetValue(data, principalPrimaryKeyList[index]);
                        //                 }
                        //             }
                        //         }
                        //         else
                        //         {

                        //         }
                        //     }
                        // });

                        try
                        {
                            genericRepository.Insert(data);
                            remainingCount--;
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    });
                }
            }

            //_trace.Log($"[ASSISTANT]: {JsonSerializer.Serialize(entity?.MockData)}");
        }
    }
}