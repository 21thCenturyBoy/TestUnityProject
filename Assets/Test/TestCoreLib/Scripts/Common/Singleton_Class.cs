public class Singleton_Class<T> where T : Singleton_Class<T>, new()
{
    private static T _instance;
    protected Singleton_Class() { }

    public static T Instance
    {
        get { return _instance ??= new T(); }
    }
    public static T FindInstance() => _instance;
}