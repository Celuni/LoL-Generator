using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LoL_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        StackPanel lastWindow;
        string keybinding;
        HotKey _hotKey;
        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public MainWindow()
        {
            //initiate window
            InitializeComponent();

            _hotKey = new HotKey(Properties.Settings.Default.Key, KeyModifier.Alt, OnHotKeyHandler);
            HotkeyTextBox.Text = "Alt + " + Properties.Settings.Default.Key;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            e.Cancel = true;

            Hide();
        }

        private void MoveWindow(object sender, MouseButtonEventArgs e)
        {
           DragMove();
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void SettingsButton(object sender, RoutedEventArgs e)
        {
            if (SettingsOverlay.Visibility == Visibility.Hidden)
            {
                lastWindow = (WaitingOverlay.Visibility == Visibility.Visible) ? WaitingOverlay : ChampionOverlay;

                if (WaitingOverlay.Visibility == Visibility.Visible)
                {
                    WaitingOverlay.Visibility = Visibility.Hidden;
                }
                else if (ChampionOverlay.Visibility == Visibility.Visible)
                {
                    ChampionOverlay.Visibility = Visibility.Hidden;
                }

                SettingsOverlay.Visibility = Visibility.Visible;
            }
            else if (SettingsOverlay.Visibility == Visibility.Visible)
            {
                SettingsOverlay.Visibility = Visibility.Hidden;

                if (!App.listedSets)
                {
                    lastWindow.Visibility = Visibility.Visible;
                }
                else
                {
                    ChampionOverlay.Visibility = Visibility.Visible;
                }
            }
        }

        private void OnHotKeyHandler(HotKey hotKey)
        {
            EnableCheckBox.IsChecked = (EnableCheckBox.IsChecked == true) ? false : true;
        }

        private void HotkeyTextboxClick(object sender, RoutedEventArgs e)
        {
            ToggleTextBlock.Visibility = Visibility.Visible;
            keybinding = HotkeyTextBox.Text;

            HotkeyTextBox.Text = "Alt + ";
        }

        private void ReadKeys(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (e.Key == Key.Escape)
            {
                HotkeyTextBox.Text = keybinding;

                ToggleTextBlock.Text = "Press a Key, Esc to Cancel";
                ToggleTextBlock.Visibility = Visibility.Hidden;
                Keyboard.ClearFocus();

                return;
            }

            _hotKey.Dispose();
            _hotKey = new HotKey(e.Key, KeyModifier.Alt, OnHotKeyHandler);

            if (_hotKey.result)
            {
                HotkeyTextBox.Text = "Alt + " + e.Key.ToString();

                Properties.Settings.Default.Key = e.Key;
                Properties.Settings.Default.Save();

                ToggleTextBlock.Text = "Press a Key, Esc to Cancel";
                ToggleTextBlock.Visibility = Visibility.Hidden;
                Keyboard.ClearFocus();
            }
            else
            {
                keybinding = "Alt +";
                ToggleTextBlock.Text = "Error: Invalid Key";
            }
        }

        private void ResetPagesButton(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.defRunePageIDs = default;
            Properties.Settings.Default.defItemSetIDs = default;

            ComboBoxItem defaultRunePage = (ComboBoxItem)RuneMenu.FindName("DefaultRunePage");
            defaultRunePage.Tag = default;

            ComboBoxItem defaultItemPage = (ComboBoxItem)ItemMenu.FindName("DefaultItemPage");
            defaultItemPage.Tag = default;
        }

        private void OnStartupChecked(object sender, RoutedEventArgs e)
        {
            if ((bool)WindowsStartupCheckbox.IsChecked)
            {
                if (rkApp.GetValue("LoL Generator") == null)
                {
                    rkApp.SetValue("LoL Generator", System.Reflection.Assembly.GetExecutingAssembly().Location);
                }
            }
            else
            {
                rkApp.DeleteValue("LoL Generator");
            }
        }
    }
}

