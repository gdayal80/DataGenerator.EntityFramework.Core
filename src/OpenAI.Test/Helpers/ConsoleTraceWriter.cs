namespace OpenAI.Test.Helpers
{
    using OpenAI.DataGenerator.Interfaces;
    
    public class ConsoleTraceWriter : ITraceWriter
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        public void Verbose(string message)
        {
            Console.WriteLine(message);
        }
    }
}