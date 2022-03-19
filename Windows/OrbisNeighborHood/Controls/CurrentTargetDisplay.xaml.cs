using OrbisSuite;
using OrbisSuite.Common.Database;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace OrbisNeighborHood.Controls
{
    /// <summary>
    /// Interaction logic for CurrentTargetDisplay.xaml
    /// </summary>
    public partial class CurrentTargetDisplay : UserControl
    {
        public CurrentTargetDisplay()
        {
            InitializeComponent();

            OrbisLib.Instance.Events.DBTouched += Events_DBTouched;
            OrbisLib.Instance.Events.TargetStateChanged += Events_TargetStateChanged;
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
            var CurrentTarget = OrbisLib.Instance.SelectedTarget.Info;

            if (CurrentTarget != null)
            {
                switch (CurrentTarget.Status)
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

                if (CurrentTarget.CurrentTitleID == null || !Regex.IsMatch(CurrentTarget.CurrentTitleID, @"CUSA\d{5}"))
                {
                    CurrentTargetTitleName.Text = "Unknown Title";
                    CurrentTargetTitleId.Text = "-";
                    CurrentTargetTitleImage.Source = new BitmapImage(new Uri("pack://application:,,,/OrbisNeighborHood;component/Images/DefaultTitleIcon.png"));
                }
                else
                {
                    var Title = new TMDB(CurrentTarget.CurrentTitleID);
                    Regex rgx = new Regex(@"[^0-9a-zA-Z +.:']");
                    CurrentTargetTitleName.Text = Title.Names.First();
                    CurrentTargetTitleId.Text = Title.NPTitleID;
                    var test = Title.BGM;
                    CurrentTargetTitleImage.Source = new BitmapImage(new Uri(Title.Icons.First()));
                }
            }
        }
    }
}
