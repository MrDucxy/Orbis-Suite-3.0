using OrbisNeighborHood.Controls;
using System.Windows.Controls;
using OrbisSuite;
using System;

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
            RefreshTargets();

            OrbisLib.Instance.Events.DBTouched += Events_DBTouched;
            OrbisLib.Instance.Events.TargetAvailable += Events_TargetAvailable;
            OrbisLib.Instance.Events.TargetAPIAvailable += Events_TargetAPIAvailable;
            OrbisLib.Instance.Events.TargetUnAvailable += Events_TargetUnAvailable;
            OrbisLib.Instance.Events.TargetAPIUnAvailable += Events_TargetAPIUnAvailable;
        }

        private void Events_TargetAPIUnAvailable(object? sender, TargetAPIUnAvailableEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        private void Events_TargetUnAvailable(object? sender, TargetUnAvailableEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        private void Events_TargetAPIAvailable(object? sender, TargetAPIAvailableEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        private void Events_TargetAvailable(object? sender, TargetAvailableEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        private void Events_DBTouched(object? sender, DBTouchedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        private void RefreshTargets()
        {
            TargetList.Items.Clear();

            foreach (var Target in OrbisLib.Instance.TargetManagement.TargetList)
                TargetList.Items.Add(new OrbisNeighborHood.Controls.TargetView(Target.Name));

            TargetList.Items.Add(new NewTargetView());
        }
    }
}
