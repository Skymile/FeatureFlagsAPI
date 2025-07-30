using FeatureFlagsAPI.App;

public static class Routes
{
    public static Action<string, WebApplication> GenerateCRUD<T>(ICrudController<T> controller) => 
        (name, app) => app
            .Post  (name + @"/create", controller.Create)
            .Get   (name + @"/read"  , controller.Read  )
            .Put   (name + @"/update", controller.Update)
            .Delete(name + @"/{id}"  , controller.Delete);

    public static Action<string, WebApplication> GenerateList<T>(IListController<T> controller) => 
        (name, app) => app
            .Get(name + @"/list", controller.List);
}
