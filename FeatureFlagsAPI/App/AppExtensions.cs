using FeatureFlagsAPI.Services;

namespace FeatureFlagsAPI.App;

public static class AppExtensions
{
    public static WebApplication Get(this WebApplication app, string path, Delegate handler) =>
        app.Make(path, handler, app.MapGet);

    public static WebApplication Post(this WebApplication app, string path, Delegate handler) =>
        app.Make(path, handler, app.MapPost);

    public static WebApplication Delete(this WebApplication app, string path, Delegate handler) =>
        app.Make(path, handler, app.MapDelete);

    public static WebApplication Put(this WebApplication app, string path, Delegate handler) =>
        app.Make(path, handler, app.MapPut);

    private static WebApplication Make(this WebApplication app, string path, Delegate handler, Func<string, Delegate, RouteHandlerBuilder> map)
    {
        map("/" + path.ToLowerInvariant(), handler)
            .WithName(PathConverter.ToName(path))
            .WithOpenApi();

        return app;
    }
}
