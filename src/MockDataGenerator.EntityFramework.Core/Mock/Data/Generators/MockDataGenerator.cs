namespace Mock.Data.Generators
{
    using System.Reflection;
    using Microsoft.EntityFrameworkCore;
    using Mock.Data.Interfaces;

    public class MockDataGenerator<T, K> where T : DbContext where K : class
    {
        internal T _context;
        internal K _entity;
        internal Type? exoType;
        internal dynamic exo = new System.Dynamic.ExpandoObject();
        internal ITraceWriter _trace;

        public MockDataGenerator(T context, K entity, ITraceWriter trace)
        {
            _context = context;
            _entity = entity;
            _trace = trace;
        }

        public dynamic GenerateExpandoObject()
        {
            Type objType = _entity.GetType();
            PropertyInfo[] properties = objType.GetProperties();

            foreach (var prop in properties)
            {
                ((IDictionary<String, Object>)exo).Add(prop.Name, GetTypeObject(prop.DeclaringType));
            }

            return exo;
        }

        internal object GetTypeObject(Type? declaringType)
        {
            object returnObject = new { };

            _trace.Info(declaringType?.GetType().Name!);

            switch (declaringType?.GetType().Name)
            {
                case "string":
                    returnObject = new List<string>();
                    break;
                case "integer":
                    returnObject = new List<int>(2);
                    break;
            }

            return returnObject;
        }

        public void GenerateMockData(dynamic mockDataSet)
        {

        }
    }
}