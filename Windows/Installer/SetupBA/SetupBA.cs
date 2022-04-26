using System.Windows.Threading;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using SetupBA.MVVM.View;
using SetupBA.MVVM.ViewModel;

namespace SetupBA
{
    public class SetupBA : BootstrapperApplication
    {
        // global dispatcher
        static public Dispatcher BootstrapperDispatcher { get; private set; }

        // entry point for our custom UI
        protected override void Run()
        {
            BootstrapperDispatcher = Dispatcher.CurrentDispatcher;
         
            MainViewModel viewModel = new MainViewModel(this);
            Engine.Detect();

            MainView view = new MainView();
            view.DataContext = viewModel;
            view.Closed += (sender, e) => BootstrapperDispatcher.InvokeShutdown();
            view.Show();

            Dispatcher.Run();

            Engine.Quit(0);
        }
    }
}