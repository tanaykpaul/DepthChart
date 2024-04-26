namespace DC.Domain.Interfaces
{
    public interface ILogger<T>
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);
        // Add other logging methods as needed
    }
}