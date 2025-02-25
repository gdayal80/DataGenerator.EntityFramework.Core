namespace Mock.Data.Interfaces
{
    public interface ITraceWriter
    {
        void Info(string message);

        void Verbose(string message);
    }
}