using FeatureFlagsAPI.App;
using FeatureFlagsAPI.Database;
using FeatureFlagsAPI.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlagsAPI.Tests;

public class UserControllerTests : IDisposable
{
    [Fact]
    public void IsControllerCreating()
    {
        // Act
        var result = userController.Create(db, User.Create("User1"));
        var items = userController.List(db).ToList();


        Assert.IsType<Created>(result);
        Assert.True(items.Count == 1);
        Assert.True(items[0] is { Id: 1, Name: "User1", Capabilities: { Count: 0 } });
    }

    [Fact]
    public void IsControllerReadingCapabilities()
    {
        // Arrange
        User[] users = LoadThreeUsers;
        FeatureFlag[] flags = LoadThreeFlags;
        flags[0].IsActive = true;
        flags[0].AddDependant(flags[1]);
        flags[1].AddDependant(flags[2]);
        users[0].AddCapability(flags[0]);
        IResult flagResult = flagsController.Create(db, flags[0]);
        IResult[] userResults = [.. users.Select(user => userController.Create(db, user))];


        // Act
        var items = userController.List(db).ToList();
        var item = items.FirstOrDefault(i => i.Name == "User1");
        var dependant = item?.EnumerateCapabilities().ToList() ?? [];
        var activeDependants = item?.EnumerateActiveCapabilities().ToList() ?? [];


        Assert.True(items.Count == 3);
        Assert.True(dependant.Count == 3);
        Assert.True(dependant[0] is { Id: 2, Name: "Feature2" });
        Assert.True(dependant[1] is { Id: 3, Name: "Feature3" });
        Assert.True(dependant[2] is { Id: 1, Name: "Feature1" });
        Assert.True(activeDependants.Count == 1);
        Assert.True(activeDependants[0] is { Id: 1, Name: "Feature1" });
    }

    [Fact]
    public void IsControllerUpdating()
    {
        // Arrange
        User[] users = LoadThreeUsers;
        IResult[] results = [.. users.Select(user => userController.Create(db, user))];
        var itemsBefore = userController.List(db).ToList();
        db.ChangeTracker.Clear();
        itemsBefore[0].Name = "Modified1";
        itemsBefore[1].Name = "Modified2";


        // Act
        var r1 = userController.Update(db, itemsBefore[0].Id, itemsBefore[0]);
        var r2 = userController.Update(db, itemsBefore[2].Id, itemsBefore[2]);
        var itemsAfter = userController.List(db).ToList();


        Assert.IsType<Accepted>(r1);
        Assert.IsType<Accepted>(r2);
        Assert.True(itemsAfter.Count == 3);
        Assert.True(itemsAfter[0] is { Id: 1, Name: "Modified1" });
        Assert.True(itemsAfter[1] is { Id: 2, Name: "User2"  });
        Assert.True(itemsAfter[2] is { Id: 3, Name: "User3"  });
    }

    [Fact]
    public void IsControllerDeleting()
    {
        // Arrange
        User[] flags = LoadThreeUsers;
        IResult[] results = [.. flags.Select(flag => userController.Create(db, flag))];
        db.ChangeTracker.Clear();


        // Act
        var r1 = userController.Delete(db, 1);
        var r2 = userController.Delete(db, 2);
        var items = userController.List(db).ToList();


        Assert.True(items.Count == 1);
        Assert.True(items[0] is { Id: 3, Name: "User3" });
    }

    [Fact]
    public void IsControllerGrantingAndRemovingPermissions()
    {
        // Arrange
        FeatureFlag[] flags = LoadThreeFlags;
        User[] users = LoadThreeUsers;
        IResult[] userResults  = [.. users.Select(user => userController.Create(db, user))];
        IResult[] flagsResults = [.. flags.Select(flag => flagsController.Create(db, flag))];
        db.ChangeTracker.Clear();


        // Act
        users = userController.List(db).ToArray();
        var r1 = userController.GrantPermission(db, users[0].Id, flags[0].Id);
        var r2 = userController.GrantPermission(db, users[0].Id, flags[1].Id);
        var r3 = userController.RemovePermission(db, users[0].Id, flags[1].Id);
        users = userController.List(db).ToArray();


        Assert.True(users[0].Capabilities.Count == 1);
        Assert.Equal(flags[0].Name, users[0].Capabilities.First().Name);
    }

    public UserControllerTests()
    {
        app = Startup.Init(["clean", "test"]);
        scope = app.Services.CreateScope();
        db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        userController = new UserController(app);
        flagsController = new FeatureFlagController(app);
    }

    public void Dispose() => scope.Dispose();

    private User[] LoadThreeUsers => [
        User.Create("User1"),
        User.Create("User2"),
        User.Create("User3")
    ];

    private FeatureFlag[] LoadThreeFlags => [
        FeatureFlag.Create("Feature1"),
        FeatureFlag.Create("Feature2"),
        FeatureFlag.Create("Feature3")
    ];

    private WebApplication app;
    private IServiceScope scope;
    private AppDbContext db;
    private UserController userController;
    private FeatureFlagController flagsController;
}
