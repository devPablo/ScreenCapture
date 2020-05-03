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
        public App()
        {
            var hotKeyManager = new HotKeyManager();
            var hotKey = hotKeyManager.Register(Key.Space, ModifierKeys.Control);
            hotKeyManager.KeyPressed += HotKeyManagerPressed;
        }

        private void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == Key.Space)
            {
                Console.WriteLine("Test");
            }
        }
    }
}
