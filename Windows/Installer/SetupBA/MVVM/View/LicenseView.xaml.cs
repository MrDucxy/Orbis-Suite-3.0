using SetupBA.MVVM.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace SetupBA.MVVM.View
{
    /// <summary>
    /// Interaction logic for LicenseView.xaml
    /// </summary>
    public partial class LicenseView : UserControl
    {
        public LicenseView()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as LicenseViewModel;
            dc.MainVM.CurrentView = dc.MainVM.InitialVM;
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as LicenseViewModel;
            dc.MainVM.CurrentView = dc.MainVM.LocationVM;
        }
    }
}
