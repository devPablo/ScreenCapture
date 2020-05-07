using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using GlobalHotKey;

namespace WPFApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        MainWindow mainWindow = null;
        NotifyIcon notifyIcon;

        private void Application_Startup(object sender, StartupEventArgs args)
        {
            var hotKeyManager = new HotKeyManager();
            var hotKey = hotKeyManager.Register(Key.PrintScreen, ModifierKeys.None);
            hotKeyManager.KeyPressed += HotKeyManagerPressed;

            notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = SystemIcons.Information
            };
        }

        private void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == Key.PrintScreen && Current.Windows.Count == 0)
            {
                mainWindow = new MainWindow();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            notifyIcon.Dispose();
        }
    }
}
