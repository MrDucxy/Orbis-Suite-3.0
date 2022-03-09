using OrbisNeighborHood.Core;
using OrbisNeighborHood.MVVM.ViewModel.SubView;

namespace OrbisNeighborHood.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        // Commands
        public RelayCommand DashboardViewCommand { get; set; }

        public RelayCommand TargetViewCommand { get; set; }

        public RelayCommand SettingsViewCommand { get; set; }

        // View Models
        public DashboardViewModel DashboardHomeVM { get; set; }

        public TargetViewModel TargetVM { get; set; }

        public SettingsViewModel SettingsVM { get; set; }


        //SubViews
        public RelayCommand AddTargetViewCommand { get; set; }

        public AddTargetViewModel AddTargetVM { get; set; }


        private object _currentView;
        public object CurrentView
        {
            get { return _currentView; }
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }


        public static MainViewModel Instance { get; private set; }


        public MainViewModel()
        {
            Instance = this;

            // MainViews
            DashboardHomeVM = new DashboardViewModel();
            TargetVM = new TargetViewModel();
            SettingsVM = new SettingsViewModel();

            // Sub Views
            AddTargetVM = new AddTargetViewModel();

            // Set Current View.
            CurrentView = DashboardHomeVM;

            //Set up relay commands.
            DashboardViewCommand = new RelayCommand(o =>
            {
                CurrentView = DashboardHomeVM;
            });

            TargetViewCommand = new RelayCommand(o =>
            {
                CurrentView = TargetVM;
            });

            SettingsViewCommand = new RelayCommand(o =>
            {
                CurrentView = SettingsVM;
            });

            AddTargetViewCommand = new RelayCommand(o =>
            {
                CurrentView = AddTargetVM;
            });
        }
    }
}
