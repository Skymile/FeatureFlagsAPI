using FeatureFlagsAPI.Database;

using Microsoft.EntityFrameworkCore;

namespace FeatureFlagsAPI.App;

public static class Startup
{
    public static WebApplication Init(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddDbContext<AppDbContext>(b => b
                .UseInMemoryDatabase(args.Any(i => i == "test") 
                    ? Guid.NewGuid().ToString() : "FeatureFlagsDB"
                )
            );

        var app = builder.Build();
        using var scope = app.Services.CreateScope();

        if (args.All(i => i != "clean"))
        {
            using var db = DataLoader.Load(scope
                .ServiceProvider
                .GetRequiredService<AppDbContext>());

            db.SaveChanges();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        return app;
    }
}
