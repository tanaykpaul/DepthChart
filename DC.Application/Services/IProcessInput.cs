namespace DC.Application.Services
{
    public interface IProcessInput<TEntity, T>
    {
        (TEntity?, T?) GetData(string fileContents);
    }
}