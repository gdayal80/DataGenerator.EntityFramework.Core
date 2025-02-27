namespace DataGenerator.EntityFrameworkCore.Data.Analysers
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;
    using DataGenerator.EntityFrameworkCore.Interfaces;
    using DataGenerator.EntityFrameworkCore.Types;
    
    public class EntityFrameworkAnalyser<T> where T : DbContext
    {
        private ITraceWriter _trace;

        public EntityFrameworkAnalyser(ITraceWriter trace)
        {
            _trace = trace;
        }
        
        public IEnumerable<IEntityType> GetEntityTypesFromModel(T context)
        {
            return context.Model.GetEntityTypes();
        }

        public Entity AnalyseEntity<K>(IEnumerable<IEntityType> entityTypes) where K : class
        {
            var entityTypeName = typeof(K).Name;
            var entityType = entityTypes.Where(et => et?.DisplayName() == entityTypeName).FirstOrDefault();
            var displayName = entityType?.DisplayName();
            var entity = new Entity();
            var properties = entityType?.GetProperties()?.ToList()!;

            entity.DisplayName = displayName;
            entity.Properties = properties.Select(p => new Property
            {
                Name = p.Name,
                ClrTypeName = p.ClrType.Name,
                IsForeignKey = p.IsForeignKey(),
                IsPrimaryKey = p.IsPrimaryKey(),
                ValueGeneratedOnAdd = p.ValueGenerated == ValueGenerated.OnAdd,
                Principals = p.GetPrincipals().Select(pr => pr.DeclaringType.Name.Split('.').LastOrDefault()!)
            }).ToList();
            entity.MockData = new List<dynamic>();

            return entity;
        }
    }
}