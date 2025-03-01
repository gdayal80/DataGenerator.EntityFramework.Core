namespace DataGenerator.OpenAI.Interfaces
{
    public interface ITraceWriter
    {
        void Log(string message);

        void Verbose(string message);
    }
}