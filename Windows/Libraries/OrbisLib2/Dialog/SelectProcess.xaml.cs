using OrbisLib2.Targets;
using SimpleUI.Dialogs;
using System.Windows;

namespace OrbisLib2.Dialog
{
    /// <summary>
    /// Interaction logic for SelectProcess.xaml
    /// </summary>
    public partial class SelectProcess : SimpleDialog
    {
        public SelectProcess(Window Owner)
            : base(Owner, "Select", "Cancel", "Select Process")
        {
            InitializeComponent();

            // Get initial process list.
            RefreshProcessList();
        }

        public static SimpleDialogResult ShowDialog(Window Owner)
        {
            var dlg = new SelectProcess(Owner);
            dlg.ShowDialog();
            return dlg.Result;
        }

        private void RefreshProcessList()
        {
            Task.Run(() =>
            {
                var procList = TargetManager.SelectedTarget.Process.GetList();

                if(procList != null && procList.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProcessList.ItemsSource = procList;
                        ProcessList.Items.Refresh();
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        ProcessList.ItemsSource = null;
                    });
                }
            });
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcessList();
        }
    }
}
