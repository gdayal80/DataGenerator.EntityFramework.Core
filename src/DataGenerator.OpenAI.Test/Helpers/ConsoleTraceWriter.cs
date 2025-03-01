namespace DataGenerator.OpenAI.Test.Helpers
{
    using DataGenerator.OpenAI.Interfaces;
    
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