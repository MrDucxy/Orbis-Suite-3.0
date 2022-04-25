using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;

namespace SetupBA
{
    public class MainViewModel
    {
        //constructor
        public MainViewModel(BootstrapperApplication bootstrapper)
        {

            this.IsThinking = false;

            this.Bootstrapper = bootstrapper;
            this.Bootstrapper.ApplyComplete += this.OnApplyComplete;
            this.Bootstrapper.DetectPackageComplete += this.OnDetectPackageComplete;
            this.Bootstrapper.PlanComplete += this.OnPlanComplete;
        }

        #region Properties

        private bool installEnabled;
        public bool InstallEnabled
        {
            get { return installEnabled; }
            set
            {
                installEnabled = value;
                // RaisePropertyChanged("InstallEnabled");
            }
        }

        private bool uninstallEnabled;
        public bool UninstallEnabled
        {
            get { return uninstallEnabled; }
            set
            {
                uninstallEnabled = value;
                // RaisePropertyChanged("UninstallEnabled");
            }
        }

        private bool isThinking;
        public bool IsThinking
        {
            get { return isThinking; }
            set
            {
                isThinking = value;
                // RaisePropertyChanged("IsThinking");
            }
        }

        public BootstrapperApplication Bootstrapper { get; private set; }

        #endregion //Properties

        #region Methods

        private void InstallExecute()
        {
            IsThinking = true;
            Bootstrapper.Engine.Plan(LaunchAction.Install);
        }

        private void UninstallExecute()
        {
            IsThinking = true;
            Bootstrapper.Engine.Plan(LaunchAction.Uninstall);
        }

        private void ExitExecute()
        {
            SetupBA.BootstrapperDispatcher.InvokeShutdown();
        }

        /// <summary>
        /// Method that gets invoked when the Bootstrapper ApplyComplete event is fired.
        /// This is called after a bundle installation has completed. Make sure we updated the view.
        /// </summary>
        private void OnApplyComplete(object sender, ApplyCompleteEventArgs e)
        {
            IsThinking = false;
            InstallEnabled = false;
            UninstallEnabled = false;
        }

        /// <summary>
        /// Method that gets invoked when the Bootstrapper DetectPackageComplete event is fired.
        /// Checks the PackageId and sets the installation scenario. The PackageId is the ID
        /// specified in one of the package elements (msipackage, exepackage, msppackage,
        /// msupackage) in the WiX bundle.
        /// </summary>
        private void OnDetectPackageComplete(object sender, DetectPackageCompleteEventArgs e)
        {
            if (e.PackageId == "DummyInstallationPackageId")
            {
                if (e.State == PackageState.Absent)
                    InstallEnabled = true;

                else if (e.State == PackageState.Present)
                    UninstallEnabled = true;
            }
        }

        /// <summary>
        /// Method that gets invoked when the Bootstrapper PlanComplete event is fired.
        /// If the planning was successful, it instructs the Bootstrapper Engine to 
        /// install the packages.
        /// </summary>
        private void OnPlanComplete(object sender, PlanCompleteEventArgs e)
        {
            if (e.Status >= 0)
                Bootstrapper.Engine.Apply(System.IntPtr.Zero);
        }

        #endregion //Methods

        #region RelayCommands

        private RelayCommand installCommand;
        public RelayCommand InstallCommand
        {
            get
            {
                if (installCommand == null)
                    installCommand = new RelayCommand(() => InstallExecute(), () => InstallEnabled == true);

                return installCommand;
            }
        }

        private RelayCommand uninstallCommand;
        public RelayCommand UninstallCommand
        {
            get
            {
                if (uninstallCommand == null)
                    uninstallCommand = new RelayCommand(() => UninstallExecute(), () => UninstallEnabled == true);

                return uninstallCommand;
            }
        }

        private RelayCommand exitCommand;
        public RelayCommand ExitCommand
        {
            get
            {
                if (exitCommand == null)
                    exitCommand = new RelayCommand(() => ExitExecute());

                return exitCommand;
            }
        }

        #endregion //RelayCommands
    }
}