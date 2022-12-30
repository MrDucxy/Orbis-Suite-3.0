using OrbisLib2.Common.Dispatcher;
using OrbisLib2.Targets;
using SimpleUI.Controls;
using System.Windows;

namespace OrbisLibraryManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : SimpleWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DispatcherClient.Subscribe();
        }

        private void AttachProcess_Click(object sender, RoutedEventArgs e)
        {
            OrbisLib2.Dialog.SelectProcess.ShowDialog(GetWindow(this));
        }
    }
}
