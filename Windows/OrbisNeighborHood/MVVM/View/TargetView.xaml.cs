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

namespace OrbisNeighborHood.MVVM.View
{
    /// <summary>
    /// Interaction logic for TargetView.xaml
    /// </summary>
    public partial class TargetView : UserControl
    {
        public TargetView()
        {
            InitializeComponent();

            Target1.TargetStatus = 2;
            Target1.IsDefault = true;
            Target1.ConsoleModel = 0;

            Target2.TargetStatus = 0;
            Target2.IsDefault = false;
            Target2.ConsoleModel = 2;

            Target3.TargetStatus = 1;
            Target3.IsDefault = false;
            Target3.ConsoleModel = 1;

        }
    }
}
