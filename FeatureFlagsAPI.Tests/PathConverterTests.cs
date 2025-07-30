using FeatureFlagsAPI.Services;

namespace FeatureFlagsAPI.Tests;

public class PathConverterTests
{
    [Fact]
    public void IsNameToPathCorrect()
    {
        (string path, string name)[] values = [
            ("features/list"                   , "GetFeaturesList"),
            ("features/detail/{id}"            , "GetFeaturesDetailId"),
            ("features/activate/{id}/{seconds}", "GetFeaturesActivateIdSeconds"),
            ("features/activate/{id}"          , "GetFeaturesActivateId"),
            ("features/deactivate/{id}"        , "GetFeaturesDeactivateId"),
            ("features/dependant/{id}"         , "GetFeaturesDependantId"),
        ];


        foreach (var (path, name) in values)
            Assert.Equal(name, PathConverter.ToName(path));
    }
}
