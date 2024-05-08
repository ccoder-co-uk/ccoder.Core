namespace cCoder.Core.Data;

public interface ICoreEventManager
{
    Task RaiseEvent<T>(T forObject, string name) where T : class;
}