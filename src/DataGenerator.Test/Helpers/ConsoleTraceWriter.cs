namespace DataGenerator.Test.Helpers
{
    using DataGenerator.EntityFrameworkCore.Interfaces;
    
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