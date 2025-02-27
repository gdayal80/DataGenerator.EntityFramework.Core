namespace DataGenerator.EntityFrameworkCore.Types
{
    public class Entity
    {
        public string? DisplayName { get; set; }
        public List<Property>? Properties { get; set; }
        public List<string>? PrimaryKeys { get; set; }
        public List<dynamic>? MockData { get; set; }
    }
}