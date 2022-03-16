using OrbisNeighborHood.MVVM.ViewModel;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OrbisNeighborHood.MVVM.View.SubView
{
    /// <summary>
    /// Interaction logic for NewTargetView.xaml
    /// </summary>
    public partial class AddTargetView : UserControl
    {
        public AddTargetView()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.Instance != null)
                MainViewModel.Instance.CurrentView = MainViewModel.Instance.TargetVM;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.Instance != null)
                MainViewModel.Instance.CurrentView = MainViewModel.Instance.TargetVM;
        }
    }
}
