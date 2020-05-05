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
using PerMonitorDPI;
using System.Diagnostics;
using System.IO;

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
        private Bitmap image;

        private int screenWidth;
        private int screenHeight;

        private NotifyIcon notifyIcon;
        private string screenshotPath;
        private string screenshotName;

        public MainWindow()
        {
            InitializeComponent();
            screenWidth = Screen.PrimaryScreen.Bounds.Width;
            screenHeight = Screen.PrimaryScreen.Bounds.Height;

            mainCanvas.Cursor = System.Windows.Input.Cursors.Cross;
            InitScreenshot();
        }

        private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                StopApplication();
            }

            if (e.Key == Key.Space)
            {
                Bitmap bitmap = TakeCroppedScreenshot();
                if (SaveScreenshot(bitmap))
                {
                    mainCanvas.Visibility = Visibility.Hidden;
                    mainWindow.Visibility = Visibility.Hidden;
                    ShowNotification("ScreenCapture", screenshotName);
                    StopApplication();
                }
            }
        }

        private void InitScreenshot()
        {
            Bitmap bitmap = new Bitmap(screenWidth, screenHeight);
            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

            this.image = bitmap;
            BitmapSource bitmapSource = BitmapConversion.ToWpfBitmap(bitmap);
            mainImage.Source = bitmapSource;
        }

        private Bitmap TakeCroppedScreenshot()
        {
            System.Windows.Size sourcePoints = CalculateDPI((int)initialCanvasX, (int)initialCanvasY);
            System.Windows.Size destinationPoints = CalculateDPI((int)mainRectangle.ActualWidth, (int)mainRectangle.ActualHeight);

            Bitmap bitmap = CropBitmap(image, new System.Drawing.Rectangle((int)sourcePoints.Width, (int)sourcePoints.Height, (int)destinationPoints.Width, (int)destinationPoints.Height));
            return bitmap;
        }

        private bool SaveScreenshot(Bitmap bitmap)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "(*.png)|*.png",
                FileName = "Screenshot_1"
            };

            DialogResult dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult == System.Windows.Forms.DialogResult.OK)
            {
                screenshotPath = saveFileDialog.FileName;
                screenshotName = System.IO.Path.GetFileName(screenshotPath);
                bitmap.Save(saveFileDialog.FileName);
                saveFileDialog.Dispose();
                return true;
            }
            else if (dialogResult == System.Windows.Forms.DialogResult.Cancel)
            {
                saveFileDialog.Dispose();
                return false;
            }
            return false;
        }

        private Bitmap CropBitmap(Bitmap img, System.Drawing.Rectangle cropArea)
        {
            Bitmap bmp = new Bitmap(cropArea.Width, cropArea.Height);
            using (Graphics gph = Graphics.FromImage(bmp))
            {
                gph.DrawImage(img, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), cropArea, GraphicsUnit.Pixel);
            }
            return bmp;
        }

        public System.Windows.Size CalculateDPI(int x, int y)
        {
            PresentationSource mainWindowPresentationSource = PresentationSource.FromVisual(this);
            Matrix m = mainWindowPresentationSource.CompositionTarget.TransformToDevice;
            var dpiWidthFactor = m.M11;
            var dpiHeightFactor = m.M22;
            double finalX = x * dpiWidthFactor;
            double finalY = y * dpiHeightFactor;

            return new System.Windows.Size(finalX, finalY);
        }

        private void ShowNotification(string title, string fileName)
        {
            notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = SystemIcons.Information
            };

            notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
            notifyIcon.ShowBalloonTip(5000, title, $"Screenshot is saved to {fileName}", ToolTipIcon.None);
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            Process.Start(screenshotPath);
        }

        private void MainCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainMenuBorder.Visibility = Visibility.Hidden;
            mainResolution.Visibility = Visibility.Visible;

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
                // Screen Handling
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





                // Resolution Label Handling
                System.Windows.Size actualCoordinates = CalculateDPI((int) mainRectangle.Width, (int) mainRectangle.Height);
                mainResolution.Content = (int) actualCoordinates.Width + " x " + (int) actualCoordinates.Height;
                Canvas.SetLeft(mainResolution, (initialCanvasX) + 1);
                Canvas.SetTop(mainResolution, (initialCanvasY - mainResolution.ActualHeight - mainResolution.Padding.Top) - 1);
            }
        }

        private void MainCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseDownState)
            {
                mouseDownState = false;

                // Init Menu
                mainMenuBorder.Visibility = Visibility.Visible;
                mainMenuBorder.Width = mainButton.Width * mainMenu.Children.Count;
                mainMenuBorder.Height = mainMenuBorder.Height + mainButton.Height / 2;

                // Middle
                //Canvas.SetLeft(mainMenuBorder, mainWindow.Width / 2 - mainMenuBorder.Width / 2);
                //Canvas.SetTop(mainMenuBorder, mainWindow.Height * 0.85);

                // Corner
                Canvas.SetLeft(mainMenuBorder, (initialCanvasX + mainRectangle.Width) - mainMenuBorder.Width - 1);
                Canvas.SetTop(mainMenuBorder, (initialCanvasY + mainRectangle.Height) + 1);

            }
        }

        private void MainButton_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = TakeCroppedScreenshot();
            if (SaveScreenshot(bitmap))
            {
                mainCanvas.Visibility = Visibility.Hidden;
                mainWindow.Visibility = Visibility.Hidden;
                ShowNotification("ScreenCapture", screenshotName);
                mainWindow.Close();
            }
        }

        private void StopApplication()
        {
            if (notifyIcon != null)
            {
                notifyIcon.Dispose();
            }
            mainWindow.Close();
        }
    }
}
