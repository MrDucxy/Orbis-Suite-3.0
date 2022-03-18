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
        #region Constructor

        public TargetView()
        {
            InitializeComponent();
            RefreshTargets();

            OrbisLib.Instance.Events.DBTouched += Events_DBTouched;
            OrbisLib.Instance.Events.TargetStateChanged += Events_TargetStateChanged;
        }

        #endregion

        #region Events

        private void Events_TargetStateChanged(object? sender, TargetStateChangedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        private void Events_DBTouched(object? sender, DBTouchedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        private void Target_TargetChanged(object? sender, System.Windows.RoutedEventArgs e)
        {
            Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        #endregion

        public void RefreshTargets()
        {
            TargetList.Items.Clear();

            if (OrbisLib.Instance.TargetManagement.TargetList == null)
                return;

            foreach (var Target in OrbisLib.Instance.TargetManagement.TargetList)
            {
                var targetView = new TargetPanel(Target.Name);
                targetView.TargetChanged += Target_TargetChanged;
                TargetList.Items.Add(targetView);
            }
            var newTargetView = new NewTargetPanel();
            newTargetView.TargetChanged += Target_TargetChanged;
            TargetList.Items.Add(newTargetView);
        }
    }
}
