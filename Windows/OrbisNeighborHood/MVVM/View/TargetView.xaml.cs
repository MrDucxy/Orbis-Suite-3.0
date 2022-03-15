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
            //RefreshTargets();

            OrbisLib.Instance.Events.DBTouched += Events_DBTouched;
            OrbisLib.Instance.Events.TargetStateChanged += Events_TargetStateChanged;
        }

        private void Events_TargetStateChanged(object? sender, TargetStateChangedEvent e)
        {
            //Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        private void Events_DBTouched(object? sender, DBTouchedEvent e)
        {
            //Dispatcher.Invoke(() => { RefreshTargets(); });
        }

        private void RefreshTargets()
        {
            TargetList.Items.Clear();

            if (OrbisLib.Instance.TargetManagement.TargetList == null)
                return;

            foreach (var Target in OrbisLib.Instance.TargetManagement.TargetList)
                TargetList.Items.Add(new OrbisNeighborHood.Controls.TargetView(Target.Name));

            TargetList.Items.Add(new NewTargetView());
        }
    }
}
