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
            var connStr = "server=localhost; port=3306; Database=Mock.Data.Generator.DB; uid=user; pwd=User@Viedu_123; ConvertZeroDateTime=True";
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

            MockDataGenerator<Context, User> mockDataGenerator = new MockDataGenerator<Context, User>(context, trace);
            
            mockDataGenerator.AnalyseModel();
        }

    }
}