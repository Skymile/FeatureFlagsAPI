using FeatureFlagsAPI.Models.Base;
using FeatureFlagsAPI.Services;

namespace FeatureFlagsAPI.Models;

public record FeatureFlag : ModelBase
{
    public required string Name { get; set; }

    public bool IsActive { get => isActive; set => Set(ref isActive, value, TraverseDependantTree); }
    public TimeSpan? ActivePeriod { get => activePeriod; set => Set(ref activePeriod, value); }
    public HashSet<FeatureFlag> Dependant { get; set; } = [];

    public IEnumerable<FeatureFlag> EnumerateDependants() => 
        from i in Dependant
        from j in i.EnumerateDependants().Prepend(i)
        select j;

    public static FeatureFlag Create(string featureName) =>
        !Validator.IsFeatureFlagNameValid(featureName)
            ? throw new ArgumentException($"Provided feature name: `{featureName}` is invalid. It should be alphanumeric")
            : new FeatureFlag { Name = featureName };

    public FeatureFlag AddDependant(FeatureFlag flag)
    {
        Dependant.Add(flag);
        if (Id != 0 && EnumerateDependants().Any(i => i.Id == Id))
            throw new ArgumentException("Circular dependencies are disallowed.");
        return this;
    }

    public Task LaunchTemporaryActivation()
    {
        if (activePeriod is null) 
            return Task.CompletedTask;
        lock (_lock)
            return Task.Run(async () =>
            {
                IsActive = true;
                await Task.Delay(activePeriod.Value);
                IsActive = false;
            });
    }

    private void TraverseDependantTree()
    {
        if (isActive)
            return;
        foreach (var dependant in Dependant)
            dependant.IsActive = false;
    }

    private FeatureFlag() { }

    private TimeSpan? activePeriod;
    private bool isActive;
    private object _lock = new();
}
