using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

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

        public MainWindow()
        {
            //initiate window
            InitializeComponent();

            //set the default rune page and item sets and assign the generate loudout button to a function
            DefaultRunePage.Tag = Properties.Settings.Default.runePageID;
            DefaultItemPage.Tag = Properties.Settings.Default.itemSetID;

            _hotKey = new HotKey(Properties.Settings.Default.Key, KeyModifier.Alt, OnHotKeyHandler);
            EnableCheckBox.IsChecked = Properties.Settings.Default.EnableCheckBox;
            HotkeyTextBox.Text = "Alt + " + Properties.Settings.Default.Key;
            ReanableCheckBox.IsChecked = Properties.Settings.Default.ReanableCheckBox;
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

        private void EnableCheckChange(object sender, RoutedEventArgs e)
        {
            App.autoGenerate = (EnableCheckBox.IsChecked == true) ? true : false;

            Properties.Settings.Default.EnableCheckBox = (bool)EnableCheckBox.IsChecked;
            Properties.Settings.Default.Save();
        }

        private void RenableCheckChange(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ReanableCheckBox = (bool)ReanableCheckBox.IsChecked;
            Properties.Settings.Default.Save();
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

        private void ReadKeys(object sender, System.Windows.Input.KeyEventArgs e)
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
    }
}

