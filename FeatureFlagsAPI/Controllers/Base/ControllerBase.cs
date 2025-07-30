using System.Diagnostics;

using FeatureFlagsAPI.Database;

public abstract class ControllerBase(WebApplication app)
{
    protected WebApplication AddRoutes(string name, params Action<string, WebApplication>[] actions)
    {
        for (int i = 0; i < actions.Length; i++)
            actions[i](name, app);
        return app;
    }

    protected IResult WrapWithBadRequestAndSaveChanges(AppDbContext db, Func<IResult> call)
    {
        try
        {
            return call();
        }
        catch (Exception ex)
        {
            return Results.BadRequest(Debugger.IsAttached ? ex.Message : null);
        }
        finally
        {
            db?.SaveChanges();
        }
    }
}