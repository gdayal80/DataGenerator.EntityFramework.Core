namespace Data.Database
{
    using Microsoft.EntityFrameworkCore;
    using MockDataGenerator.Program.Data.Entities;

    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        public DbSet<Address> Addresses => Set<Address>();
    }
}