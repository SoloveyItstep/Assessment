namespace SessionMVC.DI;

public class IOC
{
    public static IServiceProvider CurrentProvider { get; internal set; }

    public static T? Resolve<T>() => CurrentProvider.GetService<T>();
}
