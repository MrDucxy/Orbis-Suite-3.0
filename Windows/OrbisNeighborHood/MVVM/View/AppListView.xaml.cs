using OrbisNeighborHood.Controls;
using OrbisSuite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
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
using static OrbisSuite.TargetStateChangedEvent;

namespace OrbisNeighborHood.MVVM.View
{
    /// <summary>
    /// Interaction logic for AppListView.xaml
    /// </summary>
    public partial class AppListView : UserControl
    {
        public AppListView()
        {
            InitializeComponent();

            OrbisLib.Instance.Events.DBTouched += Events_DBTouched;
            OrbisLib.Instance.Events.TargetStateChanged += Events_TargetStateChanged;

            RefreshTargetInfo();
            RefreshAppList();
        }

        #region Properties

        public string TitleString
        {
            get { return (string)GetValue(TitleStringProperty); }
            set
            {
                SetValue(TitleStringProperty, $"Applications ({value})");
            }
        }

        public static readonly DependencyProperty TitleStringProperty =
            DependencyProperty.Register("TitleString", typeof(string), typeof(AppListView), new PropertyMetadata(string.Empty));

        #endregion

        #region Events / Refresh Target

        private void Events_TargetStateChanged(object? sender, TargetStateChangedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargetInfo(); });
        }

        private void Events_DBTouched(object? sender, DBTouchedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTargetInfo(); });
        }

        private void RefreshTargetInfo()
        {
            var CurrentTarget = OrbisLib.Instance.SelectedTarget.Info;

            if (CurrentTarget != null)
            {
                TitleString = CurrentTarget.IsDefault ? $"★{CurrentTarget.Name}" : CurrentTarget.Name;
            }
        }

        #endregion


        public void RefreshAppList()
        {
            AppList.Items.Clear();

            if (OrbisLib.Instance.TargetManagement.TargetList == null)
                return;

            for(int i = 0; i < 9; i++)
            {
                var appPanel = new AppPanel("", "");
                AppList.Items.Add(appPanel);
            }
        }
    }
}
