using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace LoL_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //initiate window
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            e.Cancel = true;

            Hide();
        }

        private void moveWindow(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void closeWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void minimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

    }
}
