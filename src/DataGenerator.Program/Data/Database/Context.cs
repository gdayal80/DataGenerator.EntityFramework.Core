namespace DataGenerator.Program.Data.Database
{
    using Microsoft.EntityFrameworkCore;
    using DataGenerator.Program.Data.Entities;

    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        public DbSet<Address> Addresses => Set<Address>();
    }
}