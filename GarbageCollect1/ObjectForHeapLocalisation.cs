namespace GarbageCollect1;
public class ObjectForHeapLocalisation
{
    private int Count = 0;
    public ObjectForHeapLocalisation(int count)
    {
        this.Count = count;
        //Console.WriteLine("Obj: {0} in gen: {1}", Count, GC.GetGeneration(this));
    }

    ~ObjectForHeapLocalisation()
    {
        Thread.Sleep(500);
        Console.WriteLine("Finalized: {0} in gen: {1}", Count, GC.GetGeneration(this));
    }
}
