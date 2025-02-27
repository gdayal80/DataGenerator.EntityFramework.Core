namespace DataGenerator.EntityFrameworkCore.Types
{
    public class Property
    {
        public string? Name { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public bool ValueGeneratedOnAdd { get; set; }
        public string? ClrTypeName { get; set; }
        public IEnumerable<string>? Principals { get; set; }
    }
}