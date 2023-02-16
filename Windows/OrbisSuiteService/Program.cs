using OrbisSuiteService.Service;
using System.ServiceProcess;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

#if DEBUG
var service = new Service();
service.OnStartPublic(new string[0]);
#else
ServiceBase.Run(new Service());
#endif

class Service : ServiceBase
{
    private ServiceProvider _serviceProvider;
#if DEBUG
    bool RunService = true;
#endif

    public Service()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddLogging(builder => 
        {
            builder.AddSimpleConsole(options =>
            {
                options.IncludeScopes = false;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
        });

        _serviceProvider = services.BuildServiceProvider();
    }

    public void OnStartPublic(string[] args)
    {
        var logger = _serviceProvider.GetService<ILoggerFactory>() 
            .AddFile(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Orbis Suite\Logging\OrbisServiceLog.txt")
            .CreateLogger<Program>();
        var dp = new Dispatcher(logger);
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
#if DEBUG
        RunService = false;
#endif
    }
    protected override void OnPause()
    {
        Console.WriteLine("Pausing");
    }
}