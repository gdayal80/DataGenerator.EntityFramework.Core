namespace MockDataGenerator.Program.Helpers
{
    using Mock.Data.Interfaces;

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