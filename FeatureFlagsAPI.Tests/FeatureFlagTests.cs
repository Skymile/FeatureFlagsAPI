using FeatureFlagsAPI.Models;
using FeatureFlagsAPI.Services;

namespace FeatureFlagsAPI.Tests;

public class FeatureFlagTests
{
    [Fact]
    public void IsCircularDependencyDisallowed()
    {
        var f1 = FeatureFlag.Create("F1");
        var f2 = FeatureFlag.Create("F2");
        var f3 = FeatureFlag.Create("F3");

        f1.Id = 1;
        f2.Id = 2;
        f3.Id = 3;

        f1.AddDependant(f2);
        f2.AddDependant(f3);

        Assert.Throws<ArgumentException>(() => f3.AddDependant(f1));
    }

    [Fact]
    public void IsFeatureNameAlphanumeric()
    {
        string[] validNames = ["Abc123", "A", "2", "o", "5o5o"];
        string[] invalidNames = ["", null!, "A-34", "%", "A b"];


        foreach (var valid in validNames)
        {
            Assert.True(Validator.IsFeatureFlagNameValid(valid));
            try
            {
                FeatureFlag.Create(valid);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        foreach (var invalid in invalidNames)
        {
            Assert.False(Validator.IsFeatureFlagNameValid(invalid));
            Assert.Throws<ArgumentException>(() => FeatureFlag.Create(invalid));
        }
    }

    [Fact]
    public void IsFeatureFlagPropagating()
    {
        var flag1 = FeatureFlag.Create("A1");
        var flag2 = FeatureFlag.Create("A2");
        flag1.Id = 1;
        flag2.Id = 2;
        flag1.AddDependant(flag2);
        

        flag1.IsActive = true;
        flag2.IsActive = true;
        flag1.IsActive = false;
        

        Assert.True(!flag1.IsActive);
        Assert.True(!flag2.IsActive);
    }

    [Fact]
    public void IsFeatureFlagNotPropagating()
    {
        var flag1 = FeatureFlag.Create("A1");
        var flag2 = FeatureFlag.Create("A2");
        flag1.Id = 1;
        flag2.Id = 2;
        flag1.AddDependant(flag2);


        flag1.IsActive = true;
        flag2.IsActive = false;
        flag1.IsActive = true;


        Assert.True(flag1.IsActive);
        Assert.True(!flag2.IsActive);
    }
}
