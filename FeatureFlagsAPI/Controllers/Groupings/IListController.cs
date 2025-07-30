using FeatureFlagsAPI.Database;

public interface IListController<T>
{
    IEnumerable<T> List(AppDbContext db);
}
