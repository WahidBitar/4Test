namespace Shared
{
    public interface IServiceProxy
    {
        IProvider<T> GetProvider<T>();
        T GetService<T>();
    }
}