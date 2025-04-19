namespace Async1;
internal class Test
{
    public async Task<string> LoadAsync()
    {
        var client = new HttpClient();
        PrintThread(Thread.CurrentThread);
        var response = await client.GetAsync("https://google.com").ConfigureAwait(false);
        PrintThread(Thread.CurrentThread);
        await Task.Delay(1000);
        PrintThread(Thread.CurrentThread);
        await Task.Delay(1000).ConfigureAwait(false);
        PrintThread(Thread.CurrentThread);
        return await response.Content.ReadAsStringAsync();
    }

    public void PrintThread(Thread thread)
    {
        Console.WriteLine(string.Format("   Background: {0}\n", thread.IsBackground));
        Console.WriteLine(string.Format("   Thread Pool: {0}\n", thread.IsThreadPoolThread));
        Console.WriteLine(string.Format("   Thread ID: {0}\n", thread.ManagedThreadId));
        Console.WriteLine("====================================");
    }
}
