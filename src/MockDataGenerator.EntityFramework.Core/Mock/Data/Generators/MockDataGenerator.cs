namespace Mock.Data.Generators
{
    using Microsoft.EntityFrameworkCore;
    using Mock.Data.Interfaces;
    using MockDataGenerator.EntityFramework.Core.Mock.Data.Types;
    using System.Text.Json;
    using OpenAI.Chat;
    using System.Text;
    using Repositories.Generic;
    using Microsoft.EntityFrameworkCore.Metadata;
    using System.Reflection;

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
            _openAiBatchCount = 5;
        }

        public Entity AnalyseEntity<K>() where K : class
        {
            var entityTypeName = typeof(K).Name;
            var entityTypes = _context.Model.GetEntityTypes();
            var entityType = entityTypes.Where(et => et?.DisplayName() == entityTypeName).FirstOrDefault();
            var displayName = entityType?.DisplayName();
            var entity = new Entity();
            var properties = entityType?.GetProperties()?.ToList()!;

            entity.DisplayName = displayName;
            entity.Properties = properties;

            return entity;
        }

        public void GenerateMockData<K>(int noOfRows, int primaryKeyStartIndexAt, string nullableForeignKeyDefaultClrTypeName) where K : class
        {
            int batchArrSize = noOfRows / _openAiBatchCount;
            int remainder = noOfRows % _openAiBatchCount;
            List<int> batchArr = new List<int>(remainder > 0 ? batchArrSize + 1 : batchArrSize);
            Random random = new Random();

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

            foreach (var batchArrItem in batchArr)
            {
                int remainingCount = batchArrItem;

                while (remainingCount > 0)
                {
                    GenericRepository<T, K> genericRepository = new GenericRepository<T, K>(_context);
                    Type entityType = typeof(K);
                    string? displayName = entity?.DisplayName;
                    var rowStr = remainingCount == 1 ? "row" : "rows";

                    StringBuilder sbMessage = new StringBuilder($"create dummy data in JSON format for {displayName} table with {remainingCount} {rowStr} where each {displayName} has a ");

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

                        if (!(index == propertiesCount - 1) && messageAppended)
                        {
                            sbMessage.Append(", ");
                        }
                    }

                    string message = sbMessage.ToString()!;

                    _trace.Log($"Sending [MESSAGE]: {message}");

                    ChatCompletion completion = _openAiChatClient.CompleteChat(message);
                    string completionContentText = completion.Content[0].Text;
                    int startIndex = completionContentText.IndexOf("```json") + 7;
                    int length = completionContentText.LastIndexOf("```") - startIndex;
                    // var startStr = "//";
                    // var endStr = "entries";

                    completionContentText = completionContentText.Substring(startIndex, length);

                    // while (completionContentText.Contains(startStr))
                    // {
                    //     length = completionContentText.IndexOf(startStr);
                    //     completionContentText = completionContentText.Substring(0, length);
                    //     startIndex = completionContentText.IndexOf(endStr) + endStr.Length + 1;
                    //     completionContentText = completionContentText.Substring(startIndex);
                    // }

                    StringBuilder sbCompletionContent = new StringBuilder(completionContentText);
                    string completionContentString = sbCompletionContent.ToString();

                    _trace.Log($"[ASSISTANT]: {completionContentString}");

                    var deserializedMockData = JsonSerializer.Deserialize<List<K>>(completionContentString);
                    var foreignKeysProperties = entity?.Properties?.Where(p => p.IsForeignKey()).ToList();

                    deserializedMockData?.ForEach((K data) =>
                    {
                        Type dataType = data.GetType();

                        foreignKeysProperties?.ForEach((property) =>
                        {
                            var principals = property.GetPrincipals().Select(p => p.DeclaringType.Name.Split('.').LastOrDefault()).ToList();

                            if (principals.LastOrDefault() != principals.FirstOrDefault())
                            {
                                var principalEntity = _generatedEntities.Where(ge => ge?.DisplayName == principals.LastOrDefault()).FirstOrDefault();
                                var mockData = (List<dynamic>?)principalEntity?.MockData;
                                var primaryPropertyName = ((List<IProperty>?)principalEntity?.Properties)?.Where(p => p.IsPrimaryKey()).Select(p => p.Name).FirstOrDefault();
                                var foreignProperty = dataType.GetProperty(property.Name);
                                var count = mockData?.Count ?? 0;

                                if (count > 0)
                                {
                                    int index = random.Next(0, count - 1);
                                    var principalData = mockData?[index];
                                    var principalKeyProperty = (PropertyInfo?)principalData?.GetType().GetProperty(primaryPropertyName);
                                    var value = principalKeyProperty?.GetValue(principalData, null);

                                    foreignProperty?.SetValue(data, value);
                                }
                            }
                        });

                        try
                        {
                            genericRepository.Insert(data);
                            _context.SaveChanges();
                            remainingCount--;

                            //_trace.Log($"[data]: {JsonSerializer.Serialize(data)}");
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(ex.Message);
                        }
                    });

                    var typeCastedMockData = deserializedMockData?.Select((md) => (dynamic)md).ToList();

                    if (entity?.MockData?.Count() > 0)
                    {
                        entity?.MockData?.AddRange(typeCastedMockData!);
                    }
                    else
                    {
                        entity!.MockData = typeCastedMockData;
                    }

                    _generatedEntities.Add(entity!);
                }
            }
        }
    }
}