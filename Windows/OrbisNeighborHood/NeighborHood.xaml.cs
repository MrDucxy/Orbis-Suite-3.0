using OrbisLib2.Common.Dispatcher;
using SimpleUI.Controls;

namespace OrbisNeighborHood
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NeighborHood : SimpleWindow
    {
        public NeighborHood()
        {
            InitializeComponent();
            DispatcherClient.Subscribe();
        }
    }
}
