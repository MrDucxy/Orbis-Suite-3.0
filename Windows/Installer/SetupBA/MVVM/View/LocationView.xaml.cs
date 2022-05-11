using Microsoft.Tools.WindowsInstallerXml.Bootstrapper;
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
    /// Interaction logic for LocationView.xaml
    /// </summary>
    public partial class LocationView : UserControl
    {
        public LocationView()
        {
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as LocationViewModel;
            dc.MainVM.CurrentView = dc.MainVM.LicenseVM;
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as LocationViewModel;
            dc.MainVM.InstallExecute();
            dc.MainVM.CurrentView = dc.MainVM.InstallVM;
        }
    }
}
