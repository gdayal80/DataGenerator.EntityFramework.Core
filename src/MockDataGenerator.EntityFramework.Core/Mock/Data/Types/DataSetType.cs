namespace MockDataGenerator.EntityFramework.Core.Mock.Data.Types
{
    public class DataSetType<DSType>
    {
        public int Rows;
        public List<DSType> DataSet;

        public DataSetType()
        {
            Type[] types = [ typeof(DSType)! ];
            Type listType = typeof(List<>);
            Type genericListType = listType.MakeGenericType(types);

            DataSet = (List<DSType>)Activator.CreateInstance(genericListType)!;
        }
    }
}