namespace Shared
{
    public interface IProvider<T>
    {
        T Get();
    }
}