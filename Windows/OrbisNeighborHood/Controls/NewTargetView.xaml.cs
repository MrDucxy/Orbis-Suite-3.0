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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OrbisSuite;
using OrbisSuite.Dialog;

namespace OrbisNeighborHood.Controls
{
    /// <summary>
    /// Interaction logic for NewTargetView.xaml
    /// </summary>
    public partial class NewTargetView : UserControl
    {
        public NewTargetView()
        {
            InitializeComponent();
        }

        private void AddTargetElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OrbisLib.Instance.Dialogs.AddTarget(Window.GetWindow(this));
        }
    }
}
