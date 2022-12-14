using SimpleUI.Controls;
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

namespace OrbisNeighborHood.Controls
{
    /// <summary>
    /// Interaction logic for AppPanel.xaml
    /// </summary>
    public partial class AppPanel : UserControl
    {
        public AppPanel(string Name, string TitleId)
        {
            InitializeComponent();
        }

        private void AppPanelElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Check if application running.

            // Start this application.

            // Stop this application.
        }

        #region Buttons

        private void StartStop_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Visibility_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ChangeIcon_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Unknown_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Unknown2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Unknown3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion
    }
}
