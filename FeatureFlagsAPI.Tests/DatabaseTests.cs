using FeatureFlagsAPI.App;
using FeatureFlagsAPI.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlagsAPI.Tests;

public class DatabaseTests
{
    [Fact]
    public void IsFeatureFlagCorrectlySavedWithDependants()
    {
        var app = Startup.Init([]);
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


        var featureFlags = db.FeatureFlags
            .Include(i => i.Dependant)
            .ToList();

     
        Assert.True(featureFlags.Count(static i => i.Dependant.Count != 0) > 2);
    }

    [Fact]
    public void IsUserCorrectlySavedWithCapabilities()
    {
        var app = Startup.Init([]);
        var ioc = app.Services;
        using var scope = ioc.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();


        var featureFlags = db.Users
            .Include(i => i.Capabilities)
            .ToList();


        Assert.True(featureFlags.Count(static i => i.Capabilities.Count != 0) > 2);
    }
}