using FeatureFlagsAPI.App;
using FeatureFlagsAPI.Database;

Console.WriteLine("Init...");

var app = Startup.Init(args);

Console.WriteLine("Initialization succesful");

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

Console.WriteLine("Added {0} feature flags", db.FeatureFlags.Count());
Console.WriteLine("Added {0} users", db.Users.Count());

var userController = new UserController(app);
var featureFlags = new FeatureFlagController(app);

Console.WriteLine("App is running");

app.Run();
