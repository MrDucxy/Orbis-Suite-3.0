using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();

            using (Stream? stream = assembly.GetManifestResourceStream("OrbisNeighborHood.Resources.BuildString.txt"))
            using (StreamReader? reader = new StreamReader(stream))
            {
                BuildString = reader.ReadToEnd();
            }
        }

        public string BuildString
        {
            get { return (string)GetValue(BuildStringProperty); }
            set { SetValue(BuildStringProperty, value); }
        }

        public static readonly DependencyProperty BuildStringProperty =
            DependencyProperty.Register("BuildString", typeof(string), typeof(SettingsView), new PropertyMetadata(string.Empty));

    }
}
