namespace DataGenerator.Test.Generators
{
    using DataGenerator.EntityFrameworkCore.Interfaces;
    using DataGenerator.EntityFrameworkCore.Mock.Data.Generators;
    using DataGenerator.EntityFrameworkCore.Types;

    public class CustomDataGenerator : MockDataGenerator
    {
        public CustomDataGenerator(ITraceWriter trace, string openAiApiKey, string openAiModelName = "gpt-4o") : base(trace, openAiApiKey, openAiModelName)
        {

        }

        public override string GenerateMessage(Entity entity, string nullableForeignKeyDefaultClrTypeName = "Int64", int noOfRows = 2, int primaryKeyStartIndexAt = 1)
        {
            return base.GenerateMessage(entity, nullableForeignKeyDefaultClrTypeName, noOfRows, primaryKeyStartIndexAt);
        }

        public override string GenerateMockData(string message)
        {
            return string.Empty;
        }
    }
}