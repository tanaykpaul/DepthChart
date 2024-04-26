namespace DC.Application.Services
{
    public interface IConsumeInput<T>
    {
        T? GetData(string fileContents);
    }
}