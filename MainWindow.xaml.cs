using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        string keybinding;
        string botkeybinding;

        HotKey _hotKey;
        HotKey bot_hotKey;

        RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        public MainWindow()
        {
            //initiate window
            InitializeComponent();

            try
            {
                _hotKey = new HotKey(Properties.Settings.Default.Key, KeyModifier.Alt, OnHotKeyHandler);

                bot_hotKey = new HotKey(Properties.Settings.Default.BotKey, KeyModifier.Alt, CreateBotGame);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            HotkeyTextBox.Text = "Alt + " + Properties.Settings.Default.Key;
            BotHotkeyTextBox.Text = "Alt + " + Properties.Settings.Default.BotKey;
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

                if (App.champion == default || !Properties.Settings.Default.EnableInterfaceCheckBox)
                {
                    WaitingOverlay.Visibility = Visibility.Visible;
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

        //function that is attached to the generate loadout button, calls functions to generate a rune page and item set for the current champion and selected role
        async void GenerateLoadout(object sender, RoutedEventArgs e)
        {
            IntLoadoutProgress.Maximum = 19;
            IntLoadoutProgress.Visibility = Visibility.Visible;

            await App.GenerateRunePage(App.champion, App.currole);
            await App.GenerateItemPage(App.champion, App.currole);

            App.generatedFor = App.champion + App.currole;

            IntLoadoutProgress.Visibility = Visibility.Hidden;
            IntLoadoutProgress.Value = 0;
            IntLoadoutProgress.Maximum = 0;
        }

        async void CreateBotGame(HotKey hotKey)
        {
            try
            {
                while (OnlineInd.Visibility == Visibility.Visible)
                {
                    string createLobbyJson = "{\"customGameLobby\":{\"configuration\":{\"gameMode\":\"CLASSIC\",\"mapId\":11,\"mutators\": { \"id\": 1},\"spectatorPolicy\":\"NotAllowed\",\"teamSize\":5},\"lobbyName\":\"Name\",\"lobbyPassword\":\"password\"},\"isCustom\":true}";

                    await App.SendRequestAsync("POST", $"https://127.0.0.1:{App.port}/lol-lobby/v2/lobby", createLobbyJson);

                    if (Properties.Settings.Default.team == "one")
                    {
                        await App.SendRequestAsync("POST", $"https://127.0.0.1:{App.port}/lol-lobby/v1/lobby/custom/switch-teams?team=two", null);
                        Properties.Settings.Default.team = "two";
                    }
                    else
                    {
                        Properties.Settings.Default.team = "one";
                    }

                    string botsJson = await App.SendRequestAsync("GET", $"https://127.0.0.1:{App.port}/lol-lobby/v2/lobby/custom/available-bots", null);
                    List<BotInfo> botsList = JsonConvert.DeserializeObject<List<BotInfo>>(botsJson);

                    botsList.Remove(botsList.Find(x => x.id == 19));
                    botsList.Remove(botsList.Find(x => x.id == 13));
                    botsList.Remove(botsList.Find(x => x.id == 44));

                    for (int i = 0; i < 5; i++)
                    {
                        Bot newBot = new Bot(botsList);

                        string botJson = JsonConvert.SerializeObject(newBot);

                        await App.SendRequestAsync("POST", $"https://127.0.0.1:{App.port}/lol-lobby/v1/lobby/custom/bots", botJson);
                    }
                    
                    break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void AutoHotkeyChecked(object sender, RoutedEventArgs e)
        {
            if (HotkeyTextBox != null)
            {
                if (!(bool)AutoHotkeyCheckbox.IsChecked)
                {
                    if (_hotKey != null)
                    {
                        _hotKey.Dispose();
                        Properties.Settings.Default.Key = default;
                    }

                    HotkeyTextBox.Text = "Alt + " + Properties.Settings.Default.Key;
                    HotkeyTextBox.IsEnabled = false;
                }
                else
                {
                    HotkeyTextBox.IsEnabled = true;
                }
            }
        }

        private void BotHotkeyChecked(object sender, RoutedEventArgs e)
        {
            if (BotHotkeyTextBox != null)
            {
                if (!(bool)BotHotkeyCheckbox.IsChecked)
                {
                    if (bot_hotKey != null)
                    {
                        bot_hotKey.Dispose();
                        Properties.Settings.Default.BotKey = default;
                    }

                    BotHotkeyTextBox.Text = "Alt + " + Properties.Settings.Default.BotKey;
                }
            }
        }

        private void HotkeyTextboxClick(object sender, RoutedEventArgs e)
        {
            if (HotkeyTextBox.IsFocused)
            {
                ToggleTextBlock.Visibility = Visibility.Visible;
                keybinding = HotkeyTextBox.Text;

                HotkeyTextBox.Text = "Alt + " + Properties.Settings.Default.Key;
            }
            else
            {
                BotToggleTextBlock.Visibility = Visibility.Visible;
                botkeybinding = HotkeyTextBox.Text;

                BotHotkeyTextBox.Text = "Alt + " + Properties.Settings.Default.BotKey;
            }
        }

        private void ReadKeys(object sender, KeyEventArgs e)
        {
            e.Handled = true;

            if (e.Key == Key.Escape)
            {
                if (HotkeyTextBox.IsFocused)
                {
                    HotkeyTextBox.Text = keybinding;

                    ToggleTextBlock.Text = "Press a Key, Esc to Cancel";
                    ToggleTextBlock.Visibility = Visibility.Hidden;
                }
                else
                {
                    BotHotkeyTextBox.Text = botkeybinding;

                    BotToggleTextBlock.Text = "Press a Key, Esc to Cancel";
                    BotToggleTextBlock.Visibility = Visibility.Hidden;
                }
                    
                Keyboard.ClearFocus();

                return;
            }

            if (HotkeyTextBox.IsFocused)
            {
                if (_hotKey != null)
                {
                    _hotKey.Dispose();
                    Properties.Settings.Default.Key = default;
                }
                _hotKey = new HotKey(e.Key, KeyModifier.Alt, OnHotKeyHandler);
            }
            else
            {
                if (bot_hotKey != null)
                {
                    bot_hotKey.Dispose();
                    Properties.Settings.Default.BotKey = default;
                }
                bot_hotKey = new HotKey(e.Key, KeyModifier.Alt, CreateBotGame);
            }

            if ((_hotKey != null && _hotKey.result) || (bot_hotKey != null && bot_hotKey.result))
            {
                if (HotkeyTextBox.IsFocused)
                {
                    HotkeyTextBox.Text = "Alt + " + e.Key.ToString();

                    Properties.Settings.Default.Key = e.Key;

                    ToggleTextBlock.Text = "Press a Key, Esc to Cancel";
                    ToggleTextBlock.Visibility = Visibility.Hidden;
                }
                else
                {
                    BotHotkeyTextBox.Text = "Alt + " + e.Key.ToString();

                    Properties.Settings.Default.BotKey = e.Key;

                    BotToggleTextBlock.Text = "Press a Key, Esc to Cancel";
                    BotToggleTextBlock.Visibility = Visibility.Hidden;
                }
                    
                Keyboard.ClearFocus();
            }
            else
            {
                if (HotkeyTextBox.IsFocused)
                {
                    keybinding = "Alt + " + Properties.Settings.Default.Key;
                    ToggleTextBlock.Text = "Error: Invalid Key";
                }
                else
                {
                    botkeybinding = "Alt + " + Properties.Settings.Default.BotKey;
                    BotToggleTextBlock.Text = "Error: Invalid Key";
                }
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

