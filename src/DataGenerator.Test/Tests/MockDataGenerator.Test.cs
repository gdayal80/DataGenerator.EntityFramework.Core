namespace DataGenerator.Test.Tests
{
    using DataGenerator.EntityFrameworkCore.Data.Analysers;
    using DataGenerator.EntityFrameworkCore.Interfaces;
    using DataGenerator.Test.Data.Database;
    using DataGenerator.Test.Data.Entities;
    using DataGenerator.Test.Generators;
    using DataGenerator.Test.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    public class MockDataGeneratorTest
    {
        [Fact]
        public void TestGenerateMessage()
        {
            var trace = new ConsoleTraceWriter();
            var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
            CustomDataGenerator mockDataGenerator = new CustomDataGenerator(trace, openAiApiKey);
            
            GenerateAndCheckMessage<User>(mockDataGenerator, 5, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<School>(mockDataGenerator, 2, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<SchoolBranch>(mockDataGenerator, 5, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<Country>(mockDataGenerator, 5, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<State>(mockDataGenerator, 25, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<City>(mockDataGenerator, 125, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<AddressType>(mockDataGenerator, 2, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<Address>(mockDataGenerator, 125, 5, typeof(long).Name, trace);
        }

        private void GenerateAndCheckMessage<T>(CustomDataGenerator mockDataGenerator, int noOfRows, int openAiBatchSize, string nullableForeignKeyDefaultClrTypeName, ITraceWriter trace) where T : class
        {
            var connStr = Environment.GetEnvironmentVariable("LOCALHOST_MYSQL")!;
            var dbOptions = new DbContextOptionsBuilder<Context>().UseMySql(connStr, ServerVersion.AutoDetect(connStr),
                                mySqlOptionsAction: (MySqlDbContextOptionsBuilder sqlOptions) =>
                                {
                                    sqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 10,
                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                                    errorNumbersToAdd: null);
                                    sqlOptions.CommandTimeout(240);
                                }).ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning)).Options;
            var context = new Context(dbOptions);
            
            EntityFrameworkAnalyser<Context> entityFrameworkAnalyser = new EntityFrameworkAnalyser<Context>(trace);
            var entityTypes = entityFrameworkAnalyser.GetEntityTypesFromModel(context);
            var entity = entityFrameworkAnalyser.AnalyseEntity<User>(entityTypes);
            
            int batchArrSize = noOfRows / openAiBatchSize;
            int remainder = noOfRows % openAiBatchSize;
            List<int> batchArr = new List<int>(remainder > 0 ? batchArrSize + 1 : batchArrSize);

            for (int i = 0; i < batchArrSize; i++)
            {
                batchArr.Add(openAiBatchSize);
            }

            if (remainder > 0)
            {
                batchArr.Add(remainder);
            }

            foreach (var batchArrItem in batchArr)
            {
                var message = mockDataGenerator.GenerateMessage(entity!, nullableForeignKeyDefaultClrTypeName, batchArrItem);
                var valueGeneratedOnAddProperties = entity.Properties?.Where(p => p.ValueGeneratedOnAdd).ToList();
                bool flag = message.Contains(entity.DisplayName!);
                
                Assert.True(flag);

                valueGeneratedOnAddProperties?.ForEach((vgp) =>
                {
                    Assert.True(!message.Contains(vgp.Name!));
                });
            }
        }
    }
}