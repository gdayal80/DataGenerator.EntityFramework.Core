namespace DataGenerator.EntityFrameworkCore.Mock.Data.Generators
{
    using DataGenerator.EntityFrameworkCore.Interfaces;
    using DataGenerator.EntityFrameworkCore.Types;
    using OpenAI.Chat;
    using System.Text;

    public class MockDataGenerator
    {
        private ITraceWriter _trace;
        private ChatClient _openAiChatClient;

        public MockDataGenerator(ITraceWriter trace, string openAiApiKey, string openAiModelName = "gpt-4o")
        {
            _trace = trace;
            _openAiChatClient = new ChatClient(openAiModelName, openAiApiKey);
        }

        public virtual string GenerateMessage(Entity entity, string nullableForeignKeyDefaultClrTypeName = "Int64", int noOfRows = 2, int primaryKeyStartIndexAt = 1)
        {
            var properties = entity?.Properties!;
            string? displayName = entity?.DisplayName;

            StringBuilder sbMessage = new StringBuilder($"create dummy data in JSON format for {displayName} table with {noOfRows} rows where each {displayName} has a ");

            foreach (var property in properties)
            {
                int propertiesCount = properties.Count();
                int index = properties.IndexOf(property);
                string? clrTypeName = property.ClrTypeName;
                string typeName = (clrTypeName?.Equals("Nullable`1") ?? false) ? nullableForeignKeyDefaultClrTypeName : clrTypeName!;
                bool messageAppended = false;

                if (property.IsPrimaryKey && !property!.ValueGeneratedOnAdd)
                {
                    sbMessage.Append("PrimaryKey ");
                    sbMessage.Append($"{property!.Name} of type {typeName}");

                    if (property.ClrTypeName == typeof(long).Name || property.ClrTypeName == typeof(int).Name)
                    {
                        sbMessage.Append($" start at index {primaryKeyStartIndexAt}");
                    }

                    messageAppended = true;
                }
                else if (!property!.IsPrimaryKey && !property!.ValueGeneratedOnAdd && !property!.IsForeignKey)
                {
                    sbMessage.Append($"{property.Name} of type {typeName}");
                    messageAppended = true;
                }

                if (!(index == propertiesCount - 1) && messageAppended)
                {
                    sbMessage.Append(", ");
                }
            }

            string message = sbMessage.ToString()!;

            _trace.Log($"Sending [MESSAGE]: {message}");

            return message;
        }

        public virtual string GenerateMockData(string message)
        {
            ChatCompletion completion = _openAiChatClient.CompleteChat(message);
            string completionText = completion.Content[0].Text;

            _trace.Log($"[ASSISTANT]: {completionText}");

            return completionText;
        }
    }
}