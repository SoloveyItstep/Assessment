namespace GarbageCollect1;
internal class MyDisposeMain
{
    public static long FinalisedObjects = 0;
    public static long TotalTime = 0;

    public void Do()
    {
        for (int i = 0; i < 500000; i++)
        {
            //using var obj = new WithDispose();
            var obj = new WithoutDispose();
            obj.DoSomething();
        }

        double avgLifeTime = 1.0 * TotalTime / FinalisedObjects;
        Console.WriteLine("disposed/finalised objects: {0}k", FinalisedObjects / 1000);
        Console.WriteLine("Avarage lifetime: {0}ms", avgLifeTime);
    }
}
