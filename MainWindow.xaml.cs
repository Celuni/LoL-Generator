using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

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
    }
}
