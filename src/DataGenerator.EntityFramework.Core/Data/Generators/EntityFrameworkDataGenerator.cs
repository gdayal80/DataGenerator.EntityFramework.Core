namespace DataGenerator.EntityFrameworkCore.Data.Generators
{
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;
    using DataGenerator.EntityFrameworkCore.Interfaces;
    using DataGenerator.EntityFrameworkCore.Types;
    using DataGenerator.EntityFrameworkCore.Repositories.Generic;
    using Microsoft.EntityFrameworkCore.Metadata;
    using DataGenerator.EntityFrameworkCore.Data.Analysers;
    using System.Text.Json;
    using DataGenerator.EntityFrameworkCore.Mock.Data.Generators;

    public class EntityFrameworkDataGenerator<T> where T : DbContext
    {
        private T _context;
        private EntityFrameworkAnalyser<T> _analyser;
        private MockDataGenerator _generator;
        private ITraceWriter _trace;
        private IEnumerable<IEntityType> _entityTypes;
        private List<Entity> _generatedEntities;

        public EntityFrameworkDataGenerator(T context, MockDataGenerator generator, List<Entity> generatedEntities, ITraceWriter trace)
        {
            _context = context;
            _analyser = new EntityFrameworkAnalyser<T>(trace);
            _generator = generator;
            _trace = trace;
            _entityTypes = _analyser.GetEntityTypesFromModel(context);
            _generatedEntities = generatedEntities;
        }

        public async Task GenerateAndInsertData<K>(string nullableForeignKeyDefaultClrTypeName, string locale, int noOfRows = 2, int openAiBatchSize = 5, int retryCount = 3, params string[] coomonColumnsToIgnore) where K : class
        {
            int batchArrSize = noOfRows / openAiBatchSize;
            int remainder = noOfRows % openAiBatchSize;
            List<int> batchArr = new List<int>(remainder > 0 ? batchArrSize + 1 : batchArrSize);
            var entity = _analyser.AnalyseEntity<K>(_entityTypes);

            for (int i = 0; i < batchArrSize; i++)
            {
                batchArr.Add(openAiBatchSize);
            }

            if (remainder > 0)
            {
                batchArr.Add(remainder);
            }

            for (int i = 0; i < batchArr.Count(); i++)
            {
                var message = _generator.GenerateMessage(entity!, i, locale, nullableForeignKeyDefaultClrTypeName, batchArr[i], 1, coomonColumnsToIgnore);
                int count = 1;
                
                while (retryCount > 0)
                {
                    var data = _generator.GenerateMockData(message);

                    if (!string.IsNullOrEmpty(data))
                    {
                        string startStr = "```json";
                        string endStr = "```";
                        int startIndex = data.IndexOf(startStr) + startStr.Length + 1;
                        int length = data.LastIndexOf(endStr) - startIndex;

                        try
                        {
                            var deserializedMockData = JsonSerializer.Deserialize<List<K>>(data.Substring(startIndex, length))!;

                            await InsertMockDataAsync(entity, deserializedMockData);
                            
                            break;
                        }
                        catch (JsonException ex)
                        {
                            _trace.Log($"last operation failed with error {ex.Message}. retry count {count} for last operation again.");

                            count++;
                            retryCount--;
                        }
                        catch
                        {
                            throw;
                        }
                    }
                }
            }
        }

        private async Task InsertMockDataAsync<K>(Entity entity, IEnumerable<K> deserializedMockData) where K : class
        {
            Random random = new Random();
            GenericRepository<T, K> genericRepository = new GenericRepository<T, K>(_context);

            foreach (K data in deserializedMockData)
            {
                Type dataType = data.GetType();
                var foreignKeysProperties = entity?.Properties?.Where(p => p.IsForeignKey).ToList();
                var primaryKeyColumn = entity?.PrimaryKeys?.FirstOrDefault();
                var primaryKeyProperty = dataType.GetProperty(primaryKeyColumn!.Name!);

                if (primaryKeyColumn?.ClrTypeName == typeof(long).Name || primaryKeyColumn?.ClrTypeName == typeof(int).Name)
                {
                    primaryKeyProperty?.SetValue(data, 0);
                }

                foreignKeysProperties?.ForEach((property) =>
                {
                    var principals = property.Principals!;

                    if (principals.LastOrDefault() != principals.FirstOrDefault())
                    {
                        var principalEntity = _generatedEntities.Where(ge => ge?.DisplayName == principals.LastOrDefault()).FirstOrDefault();
                        var mockData = principalEntity?.MockData;
                        var primaryProperty = principalEntity?.PrimaryKeys?.FirstOrDefault();
                        var foreignProperty = dataType.GetProperty(property.Name!);
                        var count = mockData?.Count ?? 0;

                        if (count > 0)
                        {
                            int index = random.Next(0, count - 1);
                            var principalData = mockData?[index];
                            var principalKeyProperty = (PropertyInfo?)principalData?.GetType().GetProperty(primaryProperty!.Name);
                            var value = principalKeyProperty?.GetValue(principalData, null);

                            foreignProperty?.SetValue(data, value);
                        }
                        else
                        {
                            string errorMsg = $"foreign key {property.Name} in table {entity?.DisplayName} value could not be updated as table {principals.LastOrDefault()} doesn't have data.";

                            throw new Exception(errorMsg);
                        }
                    }
                });

                try
                {
                    genericRepository.Insert(data);

                    int noOfRows = await _context.SaveChangesAsync();

                    _trace.Log($"Inserted {noOfRows} row in table {entity?.DisplayName}");
                }
                catch
                {
                    throw;
                }
            }


            var typeCastedMockData = deserializedMockData?.ToList<dynamic>();

            entity!.MockData = typeCastedMockData;
            _generatedEntities.Add(entity!);
        }
    }
}