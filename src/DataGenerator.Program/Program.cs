namespace DataGenerator.Program
{
    using DataGenerator.Program.Data.Database;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using DataGenerator.Program.Data.Entities;
    using DataGenerator.Program.Helpers;
    using System.Collections.Generic;
    using DataGenerator.EntityFrameworkCore.Mock.Data.Generators;
    using DataGenerator.EntityFrameworkCore.Types;
    using DataGenerator.EntityFrameworkCore.Data.Generators;

    class Program
    {
        static async Task Main(string[] args)
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
            var trace = new ConsoleTraceWriter();
            var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;

            try
            {
                MockDataGenerator mockDataGenerator = new MockDataGenerator(trace, openAiApiKey);
                var generatedEntities = new List<Entity>();
                EntityFrameworkDataGenerator<Context> entityFrameworkDataGenerator = new EntityFrameworkDataGenerator<Context>(context, mockDataGenerator, generatedEntities, trace);
                
                await entityFrameworkDataGenerator.GenerateAndInsertData<User>(typeof(long).Name, 5);
                await entityFrameworkDataGenerator.GenerateAndInsertData<School>(typeof(long).Name, 2);
                await entityFrameworkDataGenerator.GenerateAndInsertData<SchoolBranch>(typeof(long).Name, 5);
                await entityFrameworkDataGenerator.GenerateAndInsertData<Country>(typeof(long).Name, 5);
                await entityFrameworkDataGenerator.GenerateAndInsertData<State>(typeof(long).Name, 25);
                await entityFrameworkDataGenerator.GenerateAndInsertData<City>(typeof(long).Name, 125);
                await entityFrameworkDataGenerator.GenerateAndInsertData<AddressType>(typeof(long).Name, 2);
                await entityFrameworkDataGenerator.GenerateAndInsertData<Address>(typeof(long).Name, 125);
            }
            catch (Exception ex)
            {
                trace.Log(ex.Message);
            }
        }
    }
}