namespace OpenAI.Test.Generators
{
    using OpenAI.DataGenerator.Interfaces;
    using OpenAI.DataGenerator.Mock.Data.Generators;
    using OpenAI.DataGenerator.Types;
    using OpenAI.Chat;

    public class CustomDataGenerator : MockDataGenerator
    {
        public CustomDataGenerator(ITraceWriter trace, string openAiApiKey, string openAiModelName = "gpt-4o") : base(trace, openAiApiKey, openAiModelName)
        {

        }

        public override string GenerateMessage(Entity entity, string locale, out ChatCompletionOptions completionOptions, int noOfRows = 2, string inDataValue = "", params string[] coomonColumnsToIgnore)
        {
            return base.GenerateMessage(entity, locale, out completionOptions, noOfRows, inDataValue, coomonColumnsToIgnore);
        }

        public override async Task<string> GenerateMockData(string message, ChatCompletionOptions completionOptions)
        {
            return string.Empty;
        }
    }
}