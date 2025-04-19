using Async1;

var test = new Test();
await test.LoadAsync();
test.PrintThread(Thread.CurrentThread);

Console.ReadKey();
