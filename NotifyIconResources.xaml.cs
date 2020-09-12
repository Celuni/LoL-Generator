using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LoL_Generator
{
    /// <summary>
    /// Interaction logic for NotifyIconResources.xaml
    /// </summary>
    public partial class NotifyIconResources : Page
    {
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => App.window.Visibility == Visibility.Hidden || App.window.WindowState == WindowState.Minimized || App.window.WindowState == WindowState.Normal,
                    CommandAction = () => {
                        if (App.window.Visibility == Visibility.Hidden)
                        {
                            App.window.Show();
                        }

                        if (App.window.WindowState == WindowState.Minimized)
                        {
                            App.window.WindowState = WindowState.Normal;
                        }

                        App.window.Activate();
                        App.window.Topmost = true;
                        App.window.Topmost = false;
                    }
                };
            }
        }

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => App.window.Visibility == Visibility.Visible,
                    CommandAction = () => App.window.Hide()
                };
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand 
                { 
                    CommandAction = () => Application.Current.Shutdown()
                };
            }
        }
    }

    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
