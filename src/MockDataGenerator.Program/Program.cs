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
        static async Task Main(string[] args)
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
            var openAiApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;

            // var executionStrategy = context.Database.CreateExecutionStrategy();

            // await executionStrategy.Execute(async () =>
            // {
            //     using (var transaction = await context.Database.BeginTransactionAsync())
            //     {
            //         try
            //         {
            //             MockDataGenerator<Context> mockDataGenerator = new MockDataGenerator<Context>(context, trace, openAiApiKey);

            //             mockDataGenerator.GenerateMockData<User>(5, 1, typeof(long).Name);
            //             await context.SaveChangesAsync();
            //             mockDataGenerator.GenerateMockData<School>(1, 1, typeof(long).Name);
            //             await context.SaveChangesAsync();
            //             mockDataGenerator.GenerateMockData<SchoolBranch>(5, 1, typeof(long).Name);
            //             await context.SaveChangesAsync();
            //             mockDataGenerator.GenerateMockData<Country>(5, 1, typeof(long).Name);
            //             await context.SaveChangesAsync();
            //             mockDataGenerator.GenerateMockData<State>(25, 1, typeof(long).Name);
            //             await context.SaveChangesAsync();
            //             mockDataGenerator.GenerateMockData<City>(125, 1, typeof(long).Name);
            //             await context.SaveChangesAsync();
            //             mockDataGenerator.GenerateMockData<AddressType>(2, 1, typeof(long).Name);
            //             await context.SaveChangesAsync();
            //             mockDataGenerator.GenerateMockData<Address>(125, 1, typeof(long).Name);
            //             await context.SaveChangesAsync();

            //             await transaction.CommitAsync();
            //         }
            //         catch (Exception ex)
            //         {
            //             await transaction.RollbackAsync();

            //             Console.WriteLine(ex.Message);
            //         }
            //     }
            // });

            try
            {
                MockDataGenerator<Context> mockDataGenerator = new MockDataGenerator<Context>(context, trace, openAiApiKey);

                mockDataGenerator.GenerateMockData<User>(5, 1, typeof(long).Name);
                await context.SaveChangesAsync();
                mockDataGenerator.GenerateMockData<School>(2, 1, typeof(long).Name);
                await context.SaveChangesAsync();
                mockDataGenerator.GenerateMockData<SchoolBranch>(5, 1, typeof(long).Name);
                await context.SaveChangesAsync();
                mockDataGenerator.GenerateMockData<Country>(5, 1, typeof(long).Name);
                await context.SaveChangesAsync();
                mockDataGenerator.GenerateMockData<State>(25, 1, typeof(long).Name);
                await context.SaveChangesAsync();
                mockDataGenerator.GenerateMockData<City>(125, 1, typeof(long).Name);
                await context.SaveChangesAsync();
                mockDataGenerator.GenerateMockData<AddressType>(2, 1, typeof(long).Name);
                await context.SaveChangesAsync();
                mockDataGenerator.GenerateMockData<Address>(125, 1, typeof(long).Name);
                await context.SaveChangesAsync();

                //await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                //await transaction.RollbackAsync();

                Console.WriteLine(ex.Message);
            }
        }
    }
}