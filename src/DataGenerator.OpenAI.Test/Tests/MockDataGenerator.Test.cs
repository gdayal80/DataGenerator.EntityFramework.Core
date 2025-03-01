namespace DataGenerator.OpenAI.Test.Tests
{
    using DataGenerator.OpenAI.Data.Analysers;
    using DataGenerator.OpenAI.Interfaces;
    using DataGenerator.OpenAI.Test.Data.Database;
    using DataGenerator.OpenAI.Test.Data.Entities;
    using DataGenerator.OpenAI.Test.Generators;
    using DataGenerator.OpenAI.Test.Helpers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using global::OpenAI.Chat;

    public class MockDataGeneratorTest
    {
        [Fact]
        public void TestGenerateMessage()
        {
            var trace = new ConsoleTraceWriter();
            var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
            CustomDataGenerator mockDataGenerator = new CustomDataGenerator(trace, openAiApiKey);
            string locale = "en-US";

            GenerateAndCheckMessage<User>(mockDataGenerator, locale, 5, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<School>(mockDataGenerator, locale, 2, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<SchoolBranch>(mockDataGenerator, locale, 5, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<Country>(mockDataGenerator, locale, 5, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<State>(mockDataGenerator, locale, 25, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<City>(mockDataGenerator, locale, 125, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<AddressType>(mockDataGenerator, locale, 2, 5, typeof(long).Name, trace);
            GenerateAndCheckMessage<Address>(mockDataGenerator, locale, 125, 5, typeof(long).Name, trace);
        }

        private void GenerateAndCheckMessage<T>(CustomDataGenerator mockDataGenerator, string locale, int noOfRows, int openAiBatchSize, string nullableForeignKeyDefaultClrTypeName, ITraceWriter trace) where T : class
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
            ChatCompletionOptions completionOptions;

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
                var message = mockDataGenerator.GenerateMessage(entity!, locale, out completionOptions, batchArr[i]);
                var valueGeneratedOnAddProperties = entity.PrimaryKeys?.ToList();
                bool flag = message.Contains(entity.DisplayName!);
                int? count = valueGeneratedOnAddProperties?.Count();

                Assert.True(flag);
                Assert.True(count >= 1);

                valueGeneratedOnAddProperties?.ForEach((vgp) =>
                {
                    Assert.True(!message.Contains(vgp.Name!));
                });
            }
        }
    }
}