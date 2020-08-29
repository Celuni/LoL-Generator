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
        Key lastKey;

        public MainWindow()
        {
            //initiate window
            InitializeComponent();

            HotKey _hotKey = new HotKey(Key.S, KeyModifier.Alt, OnHotKeyHandler);
        }

        private void OnHotKeyHandler(HotKey hotKey)
        {
            
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
            if (SettingsOverlay.Visibility == Visibility.Collapsed)
            {
                lastWindow = (WaitingOverlay.Visibility == Visibility.Visible) ? WaitingOverlay : ChampionOverlay;

                if (WaitingOverlay.Visibility == Visibility.Visible)
                {
                    WaitingOverlay.Visibility = Visibility.Collapsed;
                }
                else if (ChampionOverlay.Visibility == Visibility.Visible)
                {
                    ChampionOverlay.Visibility = Visibility.Collapsed;
                }

                SettingsOverlay.Visibility = Visibility.Visible;
            }
            else if (SettingsOverlay.Visibility == Visibility.Visible)
            {
                SettingsOverlay.Visibility = Visibility.Collapsed;
                lastWindow.Visibility = Visibility.Visible;
            }
        }

        private void ActivateClick(object sender, RoutedEventArgs e)
        {

        }

        private void HotkeyClick(object sender, RoutedEventArgs e)
        {
            ToggleTextBlock.Visibility = Visibility.Visible;
            keybinding = HotkeyTextBox.Text;

            HotkeyTextBox.Clear();
        }

        private void ReadKeys(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;

            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                if (e.Key == Key.Escape)
                {
                    HotkeyTextBox.Text = keybinding;
                }

                ToggleTextBlock.Visibility = Visibility.Hidden;
                Keyboard.ClearFocus();

                return;
            }

            if (lastKey != e.Key)
            {
                if (string.IsNullOrEmpty(HotkeyTextBox.Text))
                {
                    HotkeyTextBox.Text = e.Key.ToString();
                }
                else
                {
                    HotkeyTextBox.Text += " + " + e.Key.ToString();
                }
            }

            lastKey = e.Key;
        }
    }
}

