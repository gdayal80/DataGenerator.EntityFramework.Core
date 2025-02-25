namespace Mock.Data.Generators
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Mock.Data.Interfaces;
    using MockDataGenerator.EntityFramework.Core.Mock.Data.Types;
    using System.Text.Json;

    public class MockDataGenerator<T, K> where T : DbContext where K : class
    {
        internal T _context;
        internal List<Entity> _entities;
        internal ITraceWriter _trace;

        public MockDataGenerator(T context, ITraceWriter trace)
        {
            _context = context;
            _entities = new List<Entity>();
            _trace = trace;
        }

        public void AnalyseModel()
        {
            var entityTypes = _context.Model.GetEntityTypes();
            var startDateTime = DateTime.Now;
            var startDateTimeString = startDateTime.ToString("dd/MM/yyyy HH:mm:ss");

            _trace.Log($"{startDateTimeString}: Started analysing Model");

            foreach (var entityType in entityTypes)
            {
                var entity = new Entity();
                var entityProperties = new List<Property>();
                var properties = entityType.GetProperties();

                entity.DisplayName = entityType.DisplayName();
                //entity.TypeName = entityType.GetType().Name;

                foreach (var property in properties)
                {
                    Property entityProperty = new Property();
                    var principals = new List<string>();

                    entityProperty.Name = property.Name;
                    entityProperty.IsPrimaryKey = property.IsPrimaryKey();
                    entityProperty.ClrTypeName = property.ClrType.Name;
                    entityProperty.AfterSaveBehavior = (AfterSaveBehavior)property.GetAfterSaveBehavior();
                    entityProperty.IsForeignKey = property.IsForeignKey();
                    entityProperty.MaxLength = property.GetMaxLength();
                    entityProperty.Precision = property.GetPrecision();
                    entityProperty.IsNullable = property.IsNullable;
                    property.GetPrincipals().ToList().ForEach((p) =>
                    {
                        if (entityProperty.IsForeignKey)
                        {
                            string[] declaringTypeArr = p.DeclaringType.Name.Split('.');

                            principals.Add(declaringTypeArr[declaringTypeArr.Length - 1]);
                        }
                    });
                    entityProperty.Principals = principals;

                    entityProperties.Add(entityProperty);
                }

                entity.Properties = entityProperties;
                _entities.Add(entity);
            }

            var endDateTime = DateTime.Now;
            var endDateTimeString = endDateTime.ToString("dd/MM/yyyy HH:mm:ss");
            var timeElapsedString = (endDateTime - startDateTime).TotalMilliseconds;

            _trace.Log($"{JsonSerializer.Serialize(_entities)}");
            _trace.Log($"{endDateTimeString}: Finished analysing Model");
            _trace.Log($"Operation took {timeElapsedString} milli seconds");
        }

        public void GenerateMockData()
        {

        }
    }
}