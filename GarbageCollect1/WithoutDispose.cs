using System.Diagnostics;

namespace GarbageCollect1;
internal class WithoutDispose
{
    private Stopwatch stopwatch = null;

    public WithoutDispose()
    {
        stopwatch = Stopwatch.StartNew();
        stopwatch.Start();
    }

    public void DoSomething()
    {
        var num = 0;
        for (int i = 0; i < 1000; i++)
        {
            num += i;
        }
    }

    ~WithoutDispose()
    {
        stopwatch.Stop();
        Interlocked.Increment(ref MyDisposeMain.FinalisedObjects);
        Interlocked.Add(ref MyDisposeMain.TotalTime, stopwatch.ElapsedMilliseconds);
    }
}
