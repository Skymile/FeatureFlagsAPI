using FeatureFlagsAPI.Models;

namespace FeatureFlagsAPI.Database;

public static class DataLoader
{
    public static AppDbContext Load(AppDbContext context)
    {
        var read = FeatureFlag.Create("Read");
        read.IsActive = true;

        var write = FeatureFlag.Create("Write")
            .AddDependant(read);
        write.IsActive = true;

        var delete = FeatureFlag.Create("Delete")
            .AddDependant(read)
            .AddDependant(write);

        var create = FeatureFlag.Create("Create")
            .AddDependant(read)
            .AddDependant(write);
        create.IsActive = true;

        var dropDatabase = FeatureFlag.Create("DropDatabase");

        var fullRights = FeatureFlag.Create("FullRights")
            .AddDependant(read)
            .AddDependant(write)
            .AddDependant(delete)
            .AddDependant(create)
            .AddDependant(dropDatabase);
        fullRights.IsActive = true;


        var bannedUser = User.Create("Banned Sample User");

        var guestUser = User.Create("Guest Sample User")
            .AddCapability(read);
        
        var normalUser = User.Create("Normal Sample User")
            .AddCapability(read)
            .AddCapability(write);
        
        var modUser = User.Create("Mod Sample User")
            .AddCapability(delete)
            .AddCapability(create);
        
        var adminUser = User.Create("Admin Sample User")
            .AddCapability(fullRights);

        
        context.Users.AddRange(
            bannedUser, guestUser, normalUser, modUser, adminUser);
        context.FeatureFlags.AddRange(
            read, write, delete, create, dropDatabase, fullRights);

        return context;
    }
}
