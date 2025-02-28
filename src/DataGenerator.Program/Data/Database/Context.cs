namespace DataGenerator.Program.Data.Database
{
    using Microsoft.EntityFrameworkCore;
    using DataGenerator.Program.Data.Entities;

    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {

        }

        public DbSet<User> Users => Set<User>();
        public DbSet<School> Schools => Set<School>();
        public DbSet<SchoolBranch> SchoolBranches => Set<SchoolBranch>();
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<State> States => Set<State>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<AddressType> AddressTypes => Set<AddressType>();
        public DbSet<Address> Addresses => Set<Address>();
    }
}