namespace FeatureFlagsAPI.Models.Base;

public abstract record ModelBase
{
    public int Id { get; set; }

    protected void Set<T>(ref T field, T value, Action? invoke = null)
    {
        field = value;
        invoke?.Invoke();
    }
}
