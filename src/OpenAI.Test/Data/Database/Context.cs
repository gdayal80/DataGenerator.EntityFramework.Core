namespace OpenAI.Test.Data.Database
{
    using Microsoft.EntityFrameworkCore;
    using OpenAI.Test.Data.Entities;

    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        public DbSet<Address> Addresses => Set<Address>();
    }
}