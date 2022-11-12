using OrbisNeighborHood.MVVM.ViewModel;
using OrbisNeighborHood.MVVM.ViewModel.SubView;
using OrbisSuite;
using OrbisSuite.Common.Database;
using SimpleUI.Controls;
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

namespace OrbisNeighborHood.MVVM.View.SubView
{
    /// <summary>
    /// Interaction logic for EditTargetView.xaml
    /// </summary>
    public partial class EditTargetView : UserControl
    {
        private TargetInfo _thisTarget;
        public EditTargetView()
        {
            InitializeComponent();
            _thisTarget = new TargetInfo();
        }

        #region Target Info

        private void TargetName_Loaded(object sender, RoutedEventArgs e)
        {
            var Textbox = (SimpleTextBox)sender;
            Textbox.Text = _thisTarget.Name;
        }

        private void TargetName_LostFocus(object sender, RoutedEventArgs e)
        {
            var Textbox = (SimpleTextBox)sender;
            _thisTarget.Name = Textbox.Text;
        }

        private void TargetIPAddress_Loaded(object sender, RoutedEventArgs e)
        {
            var Textbox = (SimpleTextBox)sender;
            Textbox.Text = _thisTarget.IPAddress;
        }

        private void TargetIPAddress_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9^.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TargetIPAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            Regex regex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
            var Textbox = (SimpleTextBox)sender;

            if (regex.IsMatch(Textbox.Text))
                _thisTarget.IPAddress = Textbox.Text;
            else
                Textbox.Text = "";
        }

        private void TargetPayloadPort_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void TargetPayloadPort_Loaded(object sender, RoutedEventArgs e)
        {
            var Textbox = (SimpleTextBox)sender;
            Textbox.Text = _thisTarget.PayloadPort.ToString();
        }

        private void TargetPayloadPort_LostFocus(object sender, RoutedEventArgs e)
        {
            var Textbox = (SimpleTextBox)sender;
            int value = 0;
            if (int.TryParse(Textbox.Text, out value))
                _thisTarget.PayloadPort = value;
        }

        #endregion

        #region Target Settings

        private void ShowTitleIdLabels_Loaded(object sender, RoutedEventArgs e)
        {
            var Switch = (SimpleSwitch)sender;
            Switch.IsToggled = _thisTarget.Details.ShowTitleId;

        }

        private void ShowTitleIdLabels_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var Switch = (SimpleSwitch)sender;
            _thisTarget.Details.ShowTitleId = Switch.IsToggled;

            var currentTarget = OrbisLib.Instance.Targets[_thisTarget.Name];
            currentTarget.SetSettings(_thisTarget.Details.ShowTitleId, _thisTarget.Details.ShowDevkitPanel, _thisTarget.Details.ShowToolboxShortcut, _thisTarget.Details.ShowAppHome);
        }

        private void ShowDevkitPanel_Loaded(object sender, RoutedEventArgs e)
        {
            var Switch = (SimpleSwitch)sender;
            Switch.IsToggled = _thisTarget.Details.ShowDevkitPanel;
        }

        private void ShowDevkitPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var Switch = (SimpleSwitch)sender;
            _thisTarget.Details.ShowDevkitPanel = Switch.IsToggled;

            var currentTarget = OrbisLib.Instance.Targets[_thisTarget.Name];
            currentTarget.SetSettings(_thisTarget.Details.ShowTitleId, _thisTarget.Details.ShowDevkitPanel, _thisTarget.Details.ShowToolboxShortcut, _thisTarget.Details.ShowAppHome);
        }

        private void ShowToolboxShortcut_Loaded(object sender, RoutedEventArgs e)
        {
            var Switch = (SimpleSwitch)sender;
            Switch.IsToggled = _thisTarget.Details.ShowToolboxShortcut;
        }

        private void ShowToolboxShortcut_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var Switch = (SimpleSwitch)sender;
            _thisTarget.Details.ShowToolboxShortcut = Switch.IsToggled;

            var currentTarget = OrbisLib.Instance.Targets[_thisTarget.Name];
            currentTarget.SetSettings(_thisTarget.Details.ShowTitleId, _thisTarget.Details.ShowDevkitPanel, _thisTarget.Details.ShowToolboxShortcut, _thisTarget.Details.ShowAppHome);
        }

        private void ShowAppHome_Loaded(object sender, RoutedEventArgs e)
        {
            var Switch = (SimpleSwitch)sender;
            Switch.IsToggled = _thisTarget.Details.ShowAppHome;
        }

        private void ShowAppHome_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var Switch = (SimpleSwitch)sender;
            _thisTarget.Details.ShowAppHome = Switch.IsToggled;

            var currentTarget = OrbisLib.Instance.Targets[_thisTarget.Name];
            currentTarget.SetSettings(_thisTarget.Details.ShowTitleId, _thisTarget.Details.ShowDevkitPanel, _thisTarget.Details.ShowToolboxShortcut, _thisTarget.Details.ShowAppHome);
        }

        #endregion

        #region Buttons

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (MainViewModel.Instance == null)
                return;

            var dc = (EditTargetViewModel)DataContext;
            if (dc == null || dc.CallingVM == null)
            {
                MainViewModel.Instance.CurrentView = MainViewModel.Instance.DashboardHomeVM;
            }
            else
            {
                MainViewModel.Instance.CurrentView = dc.CallingVM;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_thisTarget.Name == string.Empty || _thisTarget.Name == "-")
            {
                SimpleMessageBox.ShowError(Window.GetWindow(this), "You must give the target a name.", "Failed to save the changes to this target!");
                return;
            }

            if (_thisTarget.IPAddress == string.Empty || _thisTarget.IPAddress == "-")
            {
                SimpleMessageBox.ShowError(Window.GetWindow(this), "You must give the target an IP Address.", "Failed to save the changes to this target!");
                return;
            }

            if (_thisTarget.Save())
            {
                var dc = DataContext as EditTargetViewModel;
                if (dc != null)
                    dc.DoTargetChanged();

                if (MainViewModel.Instance == null)
                    return;

                if (dc == null || dc.CallingVM == null)
                {
                    MainViewModel.Instance.CurrentView = MainViewModel.Instance.DashboardHomeVM;
                }
                else
                {
                    MainViewModel.Instance.CurrentView = dc.CallingVM;
                }
            }
            else
            {
                SimpleMessageBox.ShowError(Window.GetWindow(this), "Maybe another target exists with a similar Name or IP Address?", "Failed to save the changes to this target!");
            }
        }

        #endregion

        private void EditTarget_Loaded(object sender, RoutedEventArgs e)
        {
            var dc = ((EditTargetViewModel)DataContext);
            if (dc == null || dc.CurrentTarget == null)
            {
                _thisTarget = new TargetInfo();
            }
            else
            {
                _thisTarget = dc.CurrentTarget;
            }
        }

        private void Title_Loaded(object sender, RoutedEventArgs e)
        {
            var Textbox = (TextBlock)sender;
            Textbox.Text = $"Edit Target ({_thisTarget.Name})";
        }
    }
}
