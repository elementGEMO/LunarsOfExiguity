namespace LunarsOfExiguity;

public abstract class GenericBase<T> where T : UnityEngine.Object
{
    protected abstract string Name { get; }
    protected T Value;

    public GenericBase()
    {
        if (IsEnabled())
        {
            Initialize();
        }
    }

    protected virtual bool IsEnabled() => true;
    protected virtual void Initialize() { }
    public T Get() => Value;
}