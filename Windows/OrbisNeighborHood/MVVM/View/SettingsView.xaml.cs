using OrbisSuite.Common.DataBase;
using SimpleUI.Skins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace OrbisNeighborHood.MVVM.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();

            var assembly = Assembly.GetExecutingAssembly();

            using (Stream? stream = assembly.GetManifestResourceStream("OrbisNeighborHood.Resources.BuildString.txt"))
            using (StreamReader? reader = new StreamReader(stream))
            {
                BuildString = reader.ReadToEnd();
            }
        }

        #region General

        private void AllowOnlyInteger(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void APIPort_LostFocus(object sender, RoutedEventArgs e)
        {
            Settings.APIPort = Convert.ToInt32(((SimpleUI.Controls.SimpleTextBox)sender).Text);
        }

        private void APIPort_Loaded(object sender, RoutedEventArgs e)
        {
            ((SimpleUI.Controls.SimpleTextBox)sender).Text = Settings.APIPort.ToString();
        }

        private void FTPPort_Loaded(object sender, RoutedEventArgs e)
        {
            ((SimpleUI.Controls.SimpleTextBox)sender).Text = Settings.FTPPort.ToString();
        }

        private void FTPPort_LostFocus(object sender, RoutedEventArgs e)
        {
            Settings.FTPPort = Convert.ToInt32(((SimpleUI.Controls.SimpleTextBox)sender).Text);
        }

        private void KlogPort_Loaded(object sender, RoutedEventArgs e)
        {
            ((SimpleUI.Controls.SimpleTextBox)sender).Text = Settings.KlogPort.ToString();
        }

        private void KlogPort_LostFocus(object sender, RoutedEventArgs e)
        {
            Settings.KlogPort = Convert.ToInt32(((SimpleUI.Controls.SimpleTextBox)sender).Text);
        }

        private void COMPort_Loaded(object sender, RoutedEventArgs e)
        {
            ((SimpleUI.Controls.SimpleTextBox)sender).Text = Settings.COMPort;
        }

        private void COMPort_LostFocus(object sender, RoutedEventArgs e)
        {
            Settings.COMPort = ((SimpleUI.Controls.SimpleTextBox)sender).Text;
        }

        private void StartOnBoot_Loaded(object sender, RoutedEventArgs e)
        {
            ((SimpleUI.Controls.SimpleSwitch)sender).IsToggled = Settings.StartOnBoot;
        }

        private void StartOnBoot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings.StartOnBoot = ((SimpleUI.Controls.SimpleSwitch)sender).IsToggled;
        }

        #endregion

        #region Theme

        private void SelectTheme(string name)
        {
            var Theme = StringtoTheme(name);
            SkinResourceDictionary.ChangeTheme((SkinResourceDictionary.Themes)Theme);
            Settings.Theme = Theme;
        }

        private int StringtoTheme(string Name)
        {
            switch (Name)
            {
                case "Red": return (int)SkinResourceDictionary.Themes.Red;
                case "Blue": return (int)SkinResourceDictionary.Themes.Blue;
                default: return (int)SkinResourceDictionary.Themes.Red;
            }
        }

        private string ThemetoString(int Theme)
        {
            switch (Theme)
            {
                case (int)SkinResourceDictionary.Themes.Red: return "Red";
                case (int)SkinResourceDictionary.Themes.Blue: return "Blue";
                default: return "Red";
            }
        }

        private void Theme_Loaded(object sender, RoutedEventArgs e)
        {
            var ThemeName = ThemetoString(Settings.Theme);
            ((SimpleUI.Controls.SimpleComboBox)sender).SelectedValue = ThemeName;
            SelectTheme(ThemeName);
        }

        private void Theme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectTheme((string)((SimpleUI.Controls.SimpleComboBox)sender).SelectedValue);
        }

        private void RainbowTheme_Loaded(object sender, RoutedEventArgs e)
        {
            ((SimpleUI.Controls.SimpleSwitch)sender).IsToggled = Settings.RainbowColours;
        }

        private void RainbowTheme_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings.RainbowColours = ((SimpleUI.Controls.SimpleSwitch)sender).IsToggled;
        }

        #endregion

        #region Target Details

        private void CensorIDPS_Loaded(object sender, RoutedEventArgs e)
        {
            ((SimpleUI.Controls.SimpleSwitch)sender).IsToggled = Settings.CensorIDPS;
        }

        private void CensorIDPS_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings.CensorIDPS = ((SimpleUI.Controls.SimpleSwitch)sender).IsToggled;
        }

        private void CensorPSID_Loaded(object sender, RoutedEventArgs e)
        {
            ((SimpleUI.Controls.SimpleSwitch)sender).IsToggled = Settings.CensorPSID;
        }

        private void CensorPSID_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Settings.CensorPSID = ((SimpleUI.Controls.SimpleSwitch)sender).IsToggled;
        }

        #endregion

        #region Build String
        public string BuildString
        {
            get { return (string)GetValue(BuildStringProperty); }
            set { SetValue(BuildStringProperty, value); }
        }

        public static readonly DependencyProperty BuildStringProperty =
            DependencyProperty.Register("BuildString", typeof(string), typeof(SettingsView), new PropertyMetadata(string.Empty));


        #endregion
    }
}
