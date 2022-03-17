using OrbisNeighborHood.Controls;
using System.Windows.Controls;
using OrbisSuite;
using System;

namespace OrbisNeighborHood.MVVM.View
{
    /// <summary>
    /// Interaction logic for TargetView.xaml
    /// </summary>
    public partial class TargetView : UserControl, ITargetView
    {
        public TargetView()
        {
            InitializeComponent();
            RefreshTargets();

            OrbisLib.Instance.Events.DBTouched += Events_DBTouched;
            OrbisLib.Instance.Events.TargetStateChanged += Events_TargetStateChanged;
        }

        private void Events_TargetStateChanged(object? sender, TargetStateChangedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        private void Events_DBTouched(object? sender, DBTouchedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        public void RefreshTargets()
        {
            TargetList.Items.Clear();

            if (OrbisLib.Instance.TargetManagement.TargetList == null)
                return;

            foreach (var Target in OrbisLib.Instance.TargetManagement.TargetList)
            {
                var targetView = new OrbisNeighborHood.Controls.TargetView(Target.Name);
                targetView.TargetChanged += Target_TargetChanged;
                TargetList.Items.Add(targetView);
            }
            var newTargetView = new NewTargetView();
            newTargetView.TargetChanged += Target_TargetChanged;
            TargetList.Items.Add(newTargetView);
        }

        private void Target_TargetChanged(object? sender, System.Windows.RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }
    }
}
