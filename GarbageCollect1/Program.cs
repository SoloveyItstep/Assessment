using GarbageCollect1;
using System.Collections;
using System.Text;

//StringBuilder? sb1 = new StringBuilder();
//for (int i = 0; i < 10000; i++)
//{
//    sb1.Append(i.ToString() + "Kb");
//}

//Console.WriteLine(sb1.ToString());

//StringBuilder sb = new StringBuilder();
//for(int i = 0; i < 10000; i++)
//{
//    sb.Append(i);
//    sb.Append("Kb");
//}

//Console.WriteLine(sb.ToString());


//ArrayList list = new ArrayList();
//List<int> list = new List<int>();
//for(int i = 0; i < 10000; i++)
//{
//    list.Add(i);
//}

//Console.WriteLine(list.Count);

new MyDisposeMain().Do();


Console.ReadKey();
