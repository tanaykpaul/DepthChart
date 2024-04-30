namespace DC.Application.Services
{
    public interface IProcessInput<TEntity, T>
    {
        TEntity? GetData(string fileContents);
    }
}