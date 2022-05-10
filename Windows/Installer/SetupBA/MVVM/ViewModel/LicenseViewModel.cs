using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetupBA.MVVM.ViewModel
{
    public class LicenseViewModel
    {
        public LicenseViewModel(MainViewModel mainViewModel)
        {
            MainVM = mainViewModel;
        }

        public MainViewModel MainVM { get; set; }

        private RelayCommand installCommand;
        public RelayCommand InstallCommand
        {
            get
            {
                if (installCommand == null)
                {
                    installCommand = new RelayCommand(() =>
                    {
                        MainVM.IsThinking = true;
                        MainVM.Bootstrapper.Engine.Plan(LaunchAction.Install);
                    }, () => MainVM.InstallEnabled == true);
                }

                return installCommand;
            }
        }

        private RelayCommand uninstallCommand;
        public RelayCommand UninstallCommand
        {
            get
            {
                if (uninstallCommand == null)
                    uninstallCommand = new RelayCommand(() =>
                    {
                        MainVM.IsThinking = true;
                        MainVM.Bootstrapper.Engine.Plan(LaunchAction.Uninstall);
                    }, () => MainVM.UnInstallEnabled == true);

                return uninstallCommand;
            }
        }
    }
}
