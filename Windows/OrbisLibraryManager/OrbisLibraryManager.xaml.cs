using OrbisLib2.Common.Dispatcher;
using SimpleUI.Controls;

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
    }
}
