using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace MockDataGenerator.EntityFramework.Core.Mock.Data.Types
{
    public class Entity
    {
        public string? DisplayName { get; set; }
        //public string? TypeName { get; set; }
        public List<Property>? Properties { get; set; }
    }
}