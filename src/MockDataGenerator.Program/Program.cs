namespace MockDataGenerator.Program
{
    using MockDataGenerator.Program.Data.Database;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Mock.Data.Generators;
    using MockDataGenerator.Program.Data.Entities;
    using MockDataGenerator.Program.Helpers;

    class Program
    {
        static void Main(string[] args)
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
                MockDataGenerator<Context> mockDataGenerator = new MockDataGenerator<Context>(context, trace, openAiApiKey);

                mockDataGenerator.GenerateMockData<User>(5, 1, typeof(long).Name);
                mockDataGenerator.GenerateMockData<School>(2, 1, typeof(long).Name);
                mockDataGenerator.GenerateMockData<SchoolBranch>(5, 1, typeof(long).Name);
                mockDataGenerator.GenerateMockData<Country>(5, 1, typeof(long).Name);
                mockDataGenerator.GenerateMockData<State>(25, 1, typeof(long).Name);
                mockDataGenerator.GenerateMockData<City>(125, 1, typeof(long).Name);
                mockDataGenerator.GenerateMockData<AddressType>(2, 1, typeof(long).Name);
                mockDataGenerator.GenerateMockData<Address>(125, 1, typeof(long).Name);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}