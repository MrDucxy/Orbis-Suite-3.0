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
            InitialVM = new InitialViewModel();
            LicenseVM = new LicenseViewModel();
            LocationVM = new LocationViewModel();
            InstallVM = new InstallViewModel();
            SummaryVM = new SummaryViewModel();

            // Set Current View.
            CurrentView = InitialVM;
        }

        public BootstrapperApplication Bootstrapper { get; private set; }

        // Commands
        private RelayCommand initialViewCommand;
        public RelayCommand InitialViewCommand { get { return initialViewCommand != null ? initialViewCommand : initialViewCommand = new RelayCommand(() => CurrentView = InitialVM); } }
        
        private RelayCommand licenseViewCommand;
        public RelayCommand LicenseViewCommand { get { return licenseViewCommand != null ? licenseViewCommand : licenseViewCommand = new RelayCommand(() => CurrentView = LicenseVM); } }

        private RelayCommand locationViewCommand;
        public RelayCommand LocationViewCommand { get { return locationViewCommand != null ? locationViewCommand : locationViewCommand = new RelayCommand(() => CurrentView = LocationVM); } }

        private RelayCommand installViewCommand;
        public RelayCommand InstallViewCommand { get { return installViewCommand != null ? installViewCommand : installViewCommand = new RelayCommand(() => CurrentView = InstallVM); } }

        private RelayCommand summaryViewCommand;
        public RelayCommand SummaryViewCommand { get { return summaryViewCommand != null ? summaryViewCommand : summaryViewCommand = new RelayCommand(() => CurrentView = SummaryVM); } }

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