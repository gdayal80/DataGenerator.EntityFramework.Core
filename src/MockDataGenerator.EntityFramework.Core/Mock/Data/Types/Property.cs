namespace MockDataGenerator.EntityFramework.Core.Mock.Data.Types
{
    using Microsoft.EntityFrameworkCore.Metadata;

    public class Property
    {
        public string? Name { get; set; }
        public string? ClrTypeName { get; set; }
        public bool IsPrimaryKey { get; set; }
        public AfterSaveBehavior AfterSaveBehavior { get; set; }
        public bool IsRequired { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public List<string>? Principals { get; set; }
    }
}