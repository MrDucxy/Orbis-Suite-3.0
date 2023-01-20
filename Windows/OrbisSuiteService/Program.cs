using OrbisSuiteService.Service;
using System.ServiceProcess;

#if DEBUG
var service = new Service();
service.OnStartPublic(new string[0]);
#else
    ServiceBase.Run(new Service());
#endif

class Service : ServiceBase
{
    bool RunService = true;

    public void OnStartPublic(string[] args)
    {
        Console.WriteLine("Starting");
        var dp = new Dispatcher();
#if DEBUG
        while (RunService) {  Thread.Sleep(10); }
#endif
    }

    protected override void OnStart(string[] args)
    {
        OnStartPublic(args);
    }
    protected override void OnStop()
    {
        Console.WriteLine("Stopping");
        RunService = false;
    }
    protected override void OnPause()
    {
        Console.WriteLine("Pausing");
    }
}