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
    /// Interaction logic for AppLauncherButton.xaml
    /// </summary>
    public partial class AppLauncherButton : UserControl
    {
        public AppLauncherButton()
        {
            InitializeComponent();
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AppLauncherButton), new PropertyMetadata(string.Empty));

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(AppLauncherButton), new PropertyMetadata("/OrbisNeighborHood;component/Images/Icons/OrbisTaskbarApp.ico"));

        private void AppLauncherButtonElement_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
