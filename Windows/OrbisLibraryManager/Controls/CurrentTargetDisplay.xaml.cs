using OrbisLib2.Common.Database.Types;
using OrbisLib2.General;
using OrbisLib2.Targets;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OrbisLibraryManager.Controls
{
    /// <summary>
    /// Interaction logic for CurrentTargetDisplay.xaml
    /// </summary>
    public partial class CurrentTargetDisplay : UserControl
    {
        public CurrentTargetDisplay()
        {
            InitializeComponent();

            Events.DBTouched += Events_DBTouched;
            Events.TargetStateChanged += Events_TargetStateChanged;
            RefreshTarget();
        }

        private void Events_TargetStateChanged(object? sender, TargetStateChangedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTarget(); });
        }

        private void Events_DBTouched(object? sender, DBTouchedEvent e)
        {
            Dispatcher.Invoke(() => { RefreshTarget(); });
        }

        private void RefreshTarget()
        {
            var CurrentTarget = TargetManager.SelectedTarget;

            if (CurrentTarget != null)
            {
                switch (CurrentTarget.Info.Status)
                {
                    case TargetStatusType.Offline:
                        CurrentTargetState.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        CurrentTargetState.ToolTip = "Offline";
                        break;

                    case TargetStatusType.Online:
                        CurrentTargetState.Fill = new SolidColorBrush(Color.FromRgb(255, 140, 0));
                        CurrentTargetState.ToolTip = "Online";
                        break;

                    case TargetStatusType.APIAvailable:
                        CurrentTargetState.Fill = new SolidColorBrush(Color.FromRgb(0, 128, 0));
                        CurrentTargetState.ToolTip = "Online & API Available";
                        break;

                    default:
                        CurrentTargetState.Fill = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                        CurrentTargetState.ToolTip = "Unknown";
                        break;
                }

                CurrentTargetName.Text = CurrentTarget.IsDefault ? $"★{CurrentTarget.Name}" : CurrentTarget.Name;

                if (CurrentTarget.Info.CurrentTitleID == null || !Regex.IsMatch(CurrentTarget.Info.CurrentTitleID, @"CUSA\d{5}"))
                {
                    CurrentTargetTitleName.Text = "Unknown Title";
                    CurrentTargetTitleId.Text = "-";
                    CurrentTargetTitleImage.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisLibraryManager;component/Images/DefaultTitleIcon.png"));
                }
                else
                {
                    var Title = new TMDB(CurrentTarget.Info.CurrentTitleID);
                    Regex rgx = new Regex(@"[^0-9a-zA-Z +.:']");
                    CurrentTargetTitleName.Text = Title.Names.First();
                    CurrentTargetTitleId.Text = Title.NPTitleID;
                    var test = Title.BGM;
                    CurrentTargetTitleImage.Source = new BitmapImage(new Uri(Title.Icons.First()));
                }
            }
        }

        private void CurrentTargetTitleImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var CurrentTarget = TargetManager.SelectedTarget;

            if (CurrentTarget != null && CurrentTarget.Info.CurrentTitleID != null && Regex.IsMatch(CurrentTarget.Info.CurrentTitleID, @"CUSA\d{5}"))
            {
                var Title = new TMDB(CurrentTarget.Info.CurrentTitleID);
                var url = $"https://store.playstation.com/product/{Title.ContentID}/";

                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }  
        }
    }
}
