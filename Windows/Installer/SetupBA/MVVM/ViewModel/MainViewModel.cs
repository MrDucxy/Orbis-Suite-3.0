using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
using SetupBA.Helpers;

namespace SetupBA.MVVM.ViewModel
{
    public class MainViewModel : PropertyNotifyBase
    {
        public MainViewModel(BootstrapperApplication bootstrapper)
        {
            Bootstrapper = bootstrapper;

            // View Models.
            InitialVM = new InitialViewModel(this);
            LicenseVM = new LicenseViewModel(this);
            LocationVM = new LocationViewModel(this);
            InstallVM = new InstallViewModel(this);
            SummaryVM = new SummaryViewModel(this);

            // Set Current View.
            CurrentView = InitialVM;
        }

        public BootstrapperApplication Bootstrapper { get; private set; }

        // View Models
        public InitialViewModel InitialVM { get; set; }
        public LicenseViewModel LicenseVM { get; set; }
        public LocationViewModel LocationVM { get; set; }
        public InstallViewModel InstallVM { get; set; }
        public SummaryViewModel SummaryVM { get; set; }

        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged("CurrentView");
            }
        }
    }
}