namespace DataGenerator.OpenAI.Test.Data.Database
{
    using Microsoft.EntityFrameworkCore;
    using DataGenerator.OpenAI.Test.Data.Entities;

    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        public DbSet<Address> Addresses => Set<Address>();
    }
}