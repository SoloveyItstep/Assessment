using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GarbageCollect1;

/// <summary>
/// some work
/// </summary>
public class DoWorkObj
{
    public void HeapLocation()
    {
        var count = 0;
        while (!Console.KeyAvailable)
        {
            new ObjectForHeapLocalisation(++count);
        }

        Console.WriteLine("Terminating process...");
    }
}
