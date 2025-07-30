using FeatureFlagsAPI.Database;

public interface ICrudController<T>
{
    IResult Create(AppDbContext db, T value);
    T? Read(AppDbContext db, int id);
    IResult Update(AppDbContext db, int id, T newValue);
    IResult Delete(AppDbContext db, int id);
}
