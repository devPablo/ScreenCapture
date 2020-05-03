using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Input;
using GlobalHotKey;

namespace WPFApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double initialCanvasX;
        private double initialCanvasY;
        private bool mouseDownState = false;

        public MainWindow()
        {
            InitializeComponent();

            var hotKeyManager = new HotKeyManager();
            var hotKey = hotKeyManager.Register(Key.PrintScreen, ModifierKeys.None);
            hotKeyManager.KeyPressed += HotKeyManagerPressed;

        }

        private void HotKeyManagerPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == Key.PrintScreen)
            {
                TakeScreenshot();

                mainWindow.Show();
                mainWindow.WindowState = WindowState.Maximized;
            }
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                mainWindow.Close();
            }
        }

        public void TakeScreenshot()
        {
            Bitmap bitmap = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

            BitmapSource bitmapSource = BitmapConversion.ToWpfBitmap(bitmap);
            mainImage.Source = bitmapSource;
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            initialCanvasX = e.GetPosition(mainCanvas).X;
            initialCanvasY = e.GetPosition(mainCanvas).Y;
            mouseDownState = true;

            Canvas.SetLeft(mainRectangle, initialCanvasX);
            Canvas.SetTop(mainRectangle, initialCanvasY);
            mainRectangle.Width = 0;
            mainRectangle.Height = 0;
        }

        private void MainCanvas_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (mouseDownState)
            {
                double eventX = e.GetPosition(mainCanvas).X;
                double eventY = e.GetPosition(mainCanvas).Y;

                if (eventX < initialCanvasX)
                {
                    Canvas.SetLeft(mainRectangle, eventX);
                }

                if (eventY < initialCanvasY)
                {
                    Canvas.SetTop(mainRectangle, eventY);
                }

                mainRectangle.Width = Math.Abs(initialCanvasX - e.GetPosition(mainCanvas).X);
                mainRectangle.Height = Math.Abs(initialCanvasY - e.GetPosition(mainCanvas).Y);
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseDownState)
            {
                mouseDownState = false;
            }
        }
    }
}
