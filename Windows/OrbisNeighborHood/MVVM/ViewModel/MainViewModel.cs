using OrbisNeighborHood.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public MainViewModel()
        {
            DashboardHomeVM = new DashboardViewModel();
            TargetVM = new TargetViewModel();
            SettingsVM = new SettingsViewModel();

            CurrentView = DashboardHomeVM;

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
        }
    }
}
