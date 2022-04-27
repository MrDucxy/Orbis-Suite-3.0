using SetupBA.MVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
