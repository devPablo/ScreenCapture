using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GlobalHotKey;

namespace WPFApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        MainWindow mainWindow = null;

        private void Application_Startup(object sender, StartupEventArgs args)
        {
            var hotKeyManager = new HotKeyManager();
            var hotKey = hotKeyManager.Register(Key.PrintScreen, ModifierKeys.None);
            hotKeyManager.KeyPressed += HotKeyManagerPressed;
        }

        private void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == Key.PrintScreen && Current.Windows.Count == 0)
            {
                mainWindow = new MainWindow();
            }
        }
    }
}
