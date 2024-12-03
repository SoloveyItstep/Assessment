using System.Diagnostics;

namespace GarbageCollect1;
internal class WithDispose : IDisposable
{
    private Stopwatch stopwatch = null;
    private bool disposed = false;

    public WithDispose()
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            stopwatch.Stop();
            Interlocked.Increment(ref MyDisposeMain.FinalisedObjects);
            Interlocked.Add(ref MyDisposeMain.TotalTime, stopwatch.ElapsedMilliseconds);

            if (disposing)
            {
                //called from user code
            }
            else
            {
                // called from finaliser
            }

            disposed = true;
        }
    }

    ~WithDispose()
    {
        Dispose(false);
    }
}
