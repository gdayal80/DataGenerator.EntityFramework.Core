namespace DataGenerator.EntityFrameworkCore.Mock.Data.Generators
{
    using DataGenerator.EntityFrameworkCore.Interfaces;
    using DataGenerator.EntityFrameworkCore.Types;
    using OpenAI.Chat;
    using System.Text;
    using System.Text.Json;

    public class MockDataGenerator
    {
        private ITraceWriter _trace;
        private ChatClient _openAiChatClient;

        public MockDataGenerator(ITraceWriter trace, string openAiApiKey, string openAiModelName = "gpt-4o")
        {
            _trace = trace;
            _openAiChatClient = new ChatClient(openAiModelName, openAiApiKey);
        }

        public virtual string GenerateMessage(Entity entity, string locale, out ChatCompletionOptions completionOptions, int noOfRows = 2, string inDataValue = "", params string[] coomonColumnsToIgnore)
        {
            var properties = entity?.Properties!;
            string? displayName = entity?.DisplayName;
            string message = string.IsNullOrEmpty(inDataValue) ? $"for locale {locale} create dummy data for table {displayName} with {noOfRows} rows" : $"for locale {locale} create dummy data for table {displayName} in {inDataValue} with {noOfRows} rows";
            JsonSchema jsonSchema = new JsonSchema { Type = "object" };
            JsonSchema jsonSchemaData = new JsonSchema { Type = "object" };
            JsonSchema jsonSchemaItems = new JsonSchema { Type = "array" };
            Dictionary<string, object> jsonSchemaDataProperties = new Dictionary<string, object>();
            Dictionary<string, object> jsonSchemaProperties = new Dictionary<string, object>();
            List<string> required = new List<string>();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            foreach (var property in properties)
            {
                string? clrTypeName = property.ClrTypeName;

                if (clrTypeName!.Equals(typeof(int).Name) || clrTypeName!.Equals(typeof(long).Name))
                {
                    clrTypeName = "Number";
                }

                if (!coomonColumnsToIgnore.Contains(property.Name) && !clrTypeName!.Equals(typeof(DateTime).Name))
                {
                    jsonSchemaProperties.Add(property.Name!, new JsonSchema { Type = $"{clrTypeName.ToLower()}", AdditionalProperties = null });
                    required.Add(property.Name!);
                }
            }

            jsonSchema.Properties = jsonSchemaProperties;
            jsonSchema.Required = required;
            jsonSchema.AdditionalProperties = false;
            jsonSchemaItems.Items = jsonSchema;
            jsonSchemaDataProperties.Add("Data", jsonSchemaItems);
            jsonSchemaData.Properties = jsonSchemaDataProperties;
            jsonSchemaData.Required = ["Data"];
            jsonSchemaData.AdditionalProperties = false;

            string jsonSchemaStr = JsonSerializer.Serialize(jsonSchemaData, options);
            byte[] bytes = Encoding.UTF8.GetBytes(jsonSchemaStr);

            completionOptions = new ChatCompletionOptions
            {
                ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(jsonSchemaFormatName: $"create_{displayName}", jsonSchema: BinaryData.FromBytes(bytes), jsonSchemaIsStrict: true)
            };

            _trace.Log($"Sending [MESSAGE]: {message}");
            _trace.Log($"Sending [jsonSchema]: {jsonSchemaStr}");

            return message;
        }

        public virtual async Task<string> GenerateMockData(string message, ChatCompletionOptions completionOptions)
        {
            ChatCompletion completion = await _openAiChatClient.CompleteChatAsync([message], completionOptions);
            string completionText = completion.Content[0].Text;

            _trace.Log($"[ASSISTANT]: {completionText}");

            return completionText;
        }
    }
}