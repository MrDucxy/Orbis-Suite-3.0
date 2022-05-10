using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using SetupBA.Helpers;
using System;

namespace SetupBA.MVVM.ViewModel
{
    /// <summary>
    /// The states of detection.
    /// </summary>
    public enum DetectionState
    {
        Absent,
        Present,
        Newer,
    }

    public enum InstallState
    {
        Install,
        Installing,
        UnInstall,
        UnInstalling,
        Complete,
        Error,
    }

    /// <summary>
    /// The model of the installation view in SetupBA.
    /// </summary>
    public class InstallViewModel : PropertyNotifyBase
    {
        public InstallViewModel(MainViewModel mainViewModel)
        {
            MainVM = mainViewModel;
        }

        public MainViewModel MainVM { get; set; }

        public string Title { 
            get
            {
                switch (MainVM.CurrentInstallState)
                {
                    case InstallState.Installing:
                        return "Installing Orbis Suite...";

                    case InstallState.UnInstalling:
                        return "Un-Installing Orbis Suite...";
                    
                    default:
                        return "Unknown";
                }
            }
        }

        private RelayCommand exitCommand;
        public RelayCommand ExitCommand
        {
            get
            {
                if (exitCommand == null)
                    exitCommand = new RelayCommand(() => SetupBA.BootstrapperDispatcher.InvokeShutdown());

                return exitCommand;
            }
        }
    }
}
