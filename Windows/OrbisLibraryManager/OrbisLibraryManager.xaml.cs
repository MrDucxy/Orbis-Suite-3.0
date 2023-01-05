using OrbisLib2.Common.Dispatcher;
using OrbisLib2.Targets;
using SimpleUI.Controls;
using System.Threading.Tasks;
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

        private void RefreshLibraryList()
        {
            Task.Run(() =>
            {
                var libraryList = TargetManager.SelectedTarget.Debug.GetLibraries();

                if (libraryList != null && libraryList.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LibraryList.ItemsSource = libraryList;
                        LibraryList.Items.Refresh();
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        LibraryList.ItemsSource = null;
                    });
                }
            });
        }

        private void AttachProcess_Click(object sender, RoutedEventArgs e)
        {
            OrbisLib2.Dialog.SelectProcess.ShowDialog(GetWindow(this));
        }

        private void DetachProcess_Click(object sender, RoutedEventArgs e)
        {
            TargetManager.SelectedTarget.Debug.Detach();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshLibraryList();
        }
    }
}
