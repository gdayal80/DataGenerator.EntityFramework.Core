namespace DataGenerator.OpenAI.Program.Data.Factory
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using DataGenerator.OpenAI.Program.Data.Database;

    public class ContextFactory : IDesignTimeDbContextFactory<Context>
    {
        public Context CreateDbContext(string[] args)
        {
            var connStr = "server=localhost; port=3306; Database=Mock.Data.Generator.DB; uid=user; pwd=User@Viedu_123; ConvertZeroDateTime=True";
            var optionsBuilder = new DbContextOptionsBuilder<Context>();

            optionsBuilder.UseMySql(connStr, ServerVersion.AutoDetect(connStr),
                                mySqlOptionsAction: (MySqlDbContextOptionsBuilder sqlOptions) =>
                                {
                                    sqlOptions.EnableRetryOnFailure(
                                    maxRetryCount: 10,
                                    maxRetryDelay: TimeSpan.FromSeconds(30),
                                    errorNumbersToAdd: null);
                                    sqlOptions.CommandTimeout(240);
                                }).ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));

            return new Context(optionsBuilder.Options);
        }
    }
}