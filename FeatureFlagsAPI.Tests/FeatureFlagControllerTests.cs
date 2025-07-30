using FeatureFlagsAPI.App;
using FeatureFlagsAPI.Database;
using FeatureFlagsAPI.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlagsAPI.Tests;

public class FeatureFlagControllerTests : IDisposable
{
    [Fact]
    public void IsControllerCreating()
    {
        // Act
        var result = controller.Create(db, FeatureFlag.Create("Feature1"));
        var items = controller.List(db).ToList();


        Assert.IsType<Created>(result);
        Assert.True(items.Count == 1);
        Assert.True(items[0] is { Id: 1, Name: "Feature1", IsActive: false });
    }

    [Fact]
    public void IsControllerReadingDependants()
    {
        // Arrange
        FeatureFlag[] flags = LoadThreeFlags;
        flags[0].AddDependant(flags[1]);
        flags[1].AddDependant(flags[2]);
        IResult result = controller.Create(db, flags[0]);

        // Act
        var items = controller.List(db).ToList();
        var item = items.FirstOrDefault(i => i.Name == "Feature1");
        var dependant = item?.EnumerateDependants().ToList() ?? [];


        Assert.True(items.Count == 3);
        Assert.True(dependant.Count == 2);
        Assert.True(dependant[0] is { Id: 2, Name: "Feature2" });
        Assert.True(dependant[1] is { Id: 3, Name: "Feature3" });
    }

    [Fact]
    public void IsControllerUpdating()
    {
        // Arrange
        FeatureFlag[] flags = LoadThreeFlags;
        IResult[] results = [.. flags.Select(flag => controller.Create(db, flag))];
        var itemsBefore = controller.List(db).ToList();
        db.ChangeTracker.Clear();
        itemsBefore[0].Name = "Modified1";
        itemsBefore[1].Name = "Modified2";
        itemsBefore[2].IsActive = true;


        // Act
        var r1 = controller.Update(db, itemsBefore[0].Id, itemsBefore[0]);
        var r2 = controller.Update(db, itemsBefore[2].Id, itemsBefore[2]);
        var itemsAfter = controller.List(db).ToList();


        Assert.IsType<Accepted>(r1);
        Assert.IsType<Accepted>(r2);
        Assert.True(itemsAfter.Count == 3);
        Assert.True(itemsAfter[0] is { Id: 1, Name: "Modified1", IsActive: false});
        Assert.True(itemsAfter[1] is { Id: 2, Name: "Feature2" , IsActive: false});
        Assert.True(itemsAfter[2] is { Id: 3, Name: "Feature3" , IsActive: true});
    }

    [Fact]
    public void IsControllerDeleting()
    {
        // Arrange
        FeatureFlag[] flags = LoadThreeFlags;
        IResult[] results = [.. flags.Select(flag => controller.Create(db, flag))];
        db.ChangeTracker.Clear();


        // Act
        var r1 = controller.Delete(db, 1);
        var r2 = controller.Delete(db, 2);
        var items = controller.List(db).ToList();


        Assert.True(items.Count == 1);
        Assert.True(items[0] is { Id: 3, Name: "Feature3" });
    }

    [Fact]
    public void IsControllerActivatingAndDeactivating()
    {
        // Arrange
        FeatureFlag[] flags = LoadThreeFlags;
        IResult[] results = [.. flags.Select(flag => controller.Create(db, flag))];
        db.ChangeTracker.Clear();


        // Act
        IResult[] activations = [
            controller.Activate(db, 1),
            controller.Activate(db, 2),
            controller.Activate(db, 3),
            controller.Deactivate(db, 3),
        ];
        var items = controller.List(db).ToList();


        Assert.True(activations.All(i => i is Ok));
        Assert.True(items.Count == 3);
        Assert.True(items[0].IsActive is true);
        Assert.True(items[1].IsActive is true);
        Assert.True(items[2].IsActive is false);
    }

    [Fact]
    public async void IsControllerActivatingTemporarily()
    {
        // Arrange
        FeatureFlag[] flags = LoadThreeFlags;
        IResult[] results = [.. flags.Select(flag => controller.Create(db, flag))];
        db.ChangeTracker.Clear();


        // Act
        FeatureFlag item = controller.Read(db, 1) ?? throw new NullReferenceException();
        item.IsActive = true;
        item.ActivePeriod = TimeSpan.FromSeconds(2);
        controller.Update(db, item.Id, item);

        var t = item
            .LaunchTemporaryActivation()
            .ContinueWith(t =>
            {
                db.SaveChanges();
                var itemsAfterAPause = controller.List(db).ToList();
                Assert.True(itemsAfterAPause[0].IsActive is false);
            });

        var itemsBeforeAPause = controller.List(db).ToList();
        Assert.True(itemsBeforeAPause[0].IsActive is true);
        await t;
    }

    public FeatureFlagControllerTests()
    {
        app = Startup.Init(["clean", "test"]);
        scope?.Dispose();
        scope = app.Services.CreateScope();
        db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        controller = new FeatureFlagController(app);
    }

    public void Dispose() => scope.Dispose();

    private FeatureFlag[] LoadThreeFlags => [
        FeatureFlag.Create("Feature1"),
        FeatureFlag.Create("Feature2"),
        FeatureFlag.Create("Feature3")
    ];
    private WebApplication app;
    private IServiceScope scope;
    private AppDbContext db;
    private FeatureFlagController controller;
}
