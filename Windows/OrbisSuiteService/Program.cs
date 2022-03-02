using System.ServiceProcess;

#if DEBUG
var service = new OrbisSuiteService();
service.OnStartPublic(new string[0]);
while (true)
{
    //Just spin wait here. 
    Thread.Sleep(1000);
}
#else
    ServiceBase.Run(new OrbisSuiteService());
#endif

class OrbisSuiteService : ServiceBase
{
    public void OnStartPublic(string[] args)
    {
        Console.WriteLine("Starting");
    }

    protected override void OnStart(string[] args)
    {
        OnStartPublic(args);
    }
    protected override void OnStop()
    {
        Console.WriteLine("Stopping");
    }
    protected override void OnPause()
    {
        Console.WriteLine("Pausing");
    }
}