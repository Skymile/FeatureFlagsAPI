using FeatureFlagsAPI.App;
using FeatureFlagsAPI.Database;
using FeatureFlagsAPI.Models;
using FeatureFlagsAPI.Services;

using Microsoft.EntityFrameworkCore;

public class FeatureFlagController : ControllerBase
    , ICrudController<FeatureFlag>
    , IListController<FeatureFlag>
{
    public FeatureFlagController(WebApplication app) : base(app) => AddRoutes("features",
            Routes.GenerateCRUD(this),
            Routes.GenerateList(this)
        )
        .Get(nameof(Activate)           , Activate)
        .Get(nameof(Deactivate)         , Deactivate)
        .Get(nameof(ActivateTemporarily), ActivateTemporarily)
        .Get(nameof(GetDependant)       , GetDependant)
        ;

    public IEnumerable<FeatureFlag> List(AppDbContext db) => db
        .FeatureFlags
        .Include(i => i.Dependant)
        .ThenInclude(i => i.Dependant)
        .AsNoTracking();

    public IResult Create(AppDbContext db, FeatureFlag value) =>
        WrapWithBadRequestAndSaveChanges(db, () =>
        {
            if (Validator.Validate([
                   (() => !Validator.IsFeatureFlagValid(value)          , "Invalid feature flag"),
                   (() => db.FeatureFlags.Any(i => i.Name == value.Name), "Provided feature flag already exists.")
                ]) is var errors and { Count: > 0 })
                return Results.ValidationProblem(errors);

            db.FeatureFlags.Add(value);
            return Results.Created();
        });

    public FeatureFlag? Read(AppDbContext db, int id) => db
        .FeatureFlags
        .Include(i => i.Dependant)
        .AsNoTracking()
        .FirstOrDefault(i => i.Id == id);

    public IResult Update(AppDbContext db, int id, FeatureFlag newValue) =>
        WrapWithBadRequestAndSaveChanges(db, () =>
        {
            if (Validator.Validate([
                    (() => !Validator.IsIdValid(id)               , "Invalid id."),
                    (() => !Validator.IsFeatureFlagValid(newValue), "Invalid feature flag.")
                ]) is var errors and { Count: > 0 })
                return Results.ValidationProblem(errors);

            newValue.Id = id;
            db.FeatureFlags.Update(newValue);
            return Results.Accepted();
        });
        
    public IResult Delete(AppDbContext db, int id) => 
        WrapWithBadRequestAndSaveChanges(db, () =>
        {
            if (Read(db, id) is FeatureFlag flag)
                db.FeatureFlags.Remove(flag);
            db.SaveChanges();
            return Results.Ok();
        });

    public IResult Activate(AppDbContext db, int id) => SetIsActive(db, id, newValue: true);

    public IResult Deactivate(AppDbContext db, int id) => SetIsActive(db, id, newValue: false);

    public IResult ActivateTemporarily(AppDbContext db, int id, int seconds) =>
        WrapWithBadRequestAndSaveChanges(db, () =>
        {
            FeatureFlag? found = db.FeatureFlags.FirstOrDefault(i => i.Id == id);
            if (Validator.Validate([
                    (() => !Validator.IsIdValid(id)                , string.Format("Invalid id ({0}).", id)),
                    (() => !Validator.IsTimeInSecondsValid(seconds), string.Format("Invalid time in seconds ({0})", seconds)),
                    (() => found is null                           , string.Format("No feature flag found with given id ({0}).", id))
                ]) is var errors and { Count: > 0 })
                return Results.ValidationProblem(errors);

            found!.ActivePeriod = TimeSpan.FromSeconds(seconds);
            found.LaunchTemporaryActivation()
                 .ContinueWith(t => db.SaveChanges());

            return Results.Ok();
        });

    public IEnumerable<FeatureFlag> GetDependant(AppDbContext db, int id) =>
        Read(db, id)?.EnumerateDependants() ?? [];

    private IResult SetIsActive(AppDbContext db, int id, bool newValue) =>
        WrapWithBadRequestAndSaveChanges(db, () =>
        {
            FeatureFlag? found = db.FeatureFlags.FirstOrDefault(i => i.Id == id);
            if (Validator.Validate([
                    (() => !Validator.IsIdValid(id), string.Format("Invalid id ({0}).", id)),
                    (() => found is null           , string.Format("No feature flag found with given id ({0}).", id))
                ]) is var errors and { Count: > 0 })
                return Results.ValidationProblem(errors);

            found!.IsActive = newValue;
            return Results.Ok();
        });
}
