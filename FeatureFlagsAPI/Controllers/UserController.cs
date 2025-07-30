using FeatureFlagsAPI.App;
using FeatureFlagsAPI.Database;
using FeatureFlagsAPI.Models;
using FeatureFlagsAPI.Services;

using Microsoft.EntityFrameworkCore;

public class UserController : ControllerBase
    , ICrudController<User>
    , IListController<User>
{
    public UserController(WebApplication app) : base(app) => AddRoutes("users",
            Routes.GenerateCRUD(this),
            Routes.GenerateList(this)
        )
        .Get(nameof(GrantPermission), GrantPermission)
        .Get(nameof(RemovePermission), RemovePermission)
        ;

    public IEnumerable<User> List(AppDbContext db) => db
        .Users
        .Include(i => i.Capabilities)
        .ThenInclude(i => i.Dependant)
        .ThenInclude(i => i.Dependant)
        .AsNoTracking();

    public IResult Create(AppDbContext db, User value) =>
        WrapWithBadRequestAndSaveChanges(db, () =>
        {
            if (Validator.Validate([
                   (() => !Validator.IsUserValid(value)          , "Invalid user"),
                   (() => db.Users.Any(i => i.Name == value.Name), "Provided user already exists.")
                ]) is var errors and { Count: > 0 })
                return Results.ValidationProblem(errors);

            db.Users.Add(value);
            return Results.Created();
        });

    public User? Read(AppDbContext db, int id) => db
        .Users
        .Include(i => i.Capabilities)
        .AsNoTracking()
        .FirstOrDefault(i => i.Id == id);

    public IResult Update(AppDbContext db, int id, User newValue) =>
        WrapWithBadRequestAndSaveChanges(db, () =>
        {
            if (Validator.Validate([
                    (() => !Validator.IsIdValid(id)        , "Invalid id."),
                    (() => !Validator.IsUserValid(newValue), "Invalid user.")
                ]) is var errors and { Count: > 0 })
                return Results.ValidationProblem(errors);

            newValue.Id = id;
            db.Users.Update(newValue);
            return Results.Accepted();
        });

    public IResult Delete(AppDbContext db, int id) =>
        WrapWithBadRequestAndSaveChanges(db, () =>
        {
            if (Read(db, id) is User user)
                db.Users.Remove(user);
            db.SaveChanges();
            return Results.Ok();
        });

    public IResult GrantPermission(AppDbContext db, int userId, int featureFlagId) => 
        SetPermission(db, userId, featureFlagId, isRemoval: false);

    public IResult RemovePermission(AppDbContext db, int userId, int featureFlagId) => 
        SetPermission(db, userId, featureFlagId, isRemoval: true);

    private IResult SetPermission(AppDbContext db, int userId, int featureFlagId, bool isRemoval) =>
        WrapWithBadRequestAndSaveChanges(db, () =>
        {
            if (Validator.Validate([
                    (() => !Validator.IsIdValid(userId)       , "User Id is invalid."),
                    (() => !Validator.IsIdValid(featureFlagId), "Feature flag Id is invalid."),
                ]) is var idErrors and { Count: > 0 })
                return Results.ValidationProblem(idErrors);

            User? user = db.Users.FirstOrDefault(i => i.Id == userId);
            FeatureFlag? flag = db.FeatureFlags.FirstOrDefault(i => i.Id == featureFlagId);

            if (Validator.Validate([
                    (() => user is null, string.Format("User not found: ({0})", userId)),
                    (() => flag is null, string.Format("Feature flag not found: ({0})", featureFlagId)),
                ]) is var errors and { Count: > 0 })
                return Results.ValidationProblem(errors);

            if (isRemoval)
                user!.RemoveCapability(flag!);
            else
                user!.AddCapability(flag!);

            return Results.Ok();
        });
}
