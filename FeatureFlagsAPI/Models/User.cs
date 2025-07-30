using FeatureFlagsAPI.Models.Base;
using FeatureFlagsAPI.Services;

namespace FeatureFlagsAPI.Models;

public record User : ModelBase
{
    public required string Name { get; set; }
    public HashSet<FeatureFlag> Capabilities { get; set; } = [];

    public static User Create(string name) =>
        !Validator.IsUserNameValid(name)
            ? throw new ArgumentException($"Provided username: `{name}` is invalid.")
            : new() { Name = name };

    public User AddCapability(FeatureFlag flag)
    {
        Capabilities.Add(flag);
        return this;
    }

    public User RemoveCapability(FeatureFlag flag)
    {
        Capabilities.Remove(flag);
        return this;
    }

    public IEnumerable<FeatureFlag> EnumerateCapabilities() =>
        from i in Capabilities
        from j in i.EnumerateDependants().Concat(Capabilities)
        select j;

    public IEnumerable<FeatureFlag> EnumerateActiveCapabilities() =>
        EnumerateCapabilities().Where(i => i.IsActive);

    private User() { }
}
