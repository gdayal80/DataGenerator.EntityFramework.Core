namespace MockDataGenerator.EntityFramework.Core.Mock.Data.Types
{
    using Microsoft.EntityFrameworkCore.Metadata;

    public class Entity
    {
        public string? DisplayName { get; set; }
        public List<IProperty>? Properties { get; set; }
        public List<dynamic>? MockData { get; set; }

        // public Entity()
        // {
        //     Type listType = typeof(List<>);
        //     Type type = typeof(K);
        //     Type genericType = listType.MakeGenericType([type]);
            
        //     MockData = (List<K>)Activator.CreateInstance(genericType)!;
        // }
    }
}