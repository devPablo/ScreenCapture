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
using System.Windows.Resources;
using System.Drawing.Imaging;

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
        private bool isDraw = false;

        private int screenWidth;
        private int screenHeight;
        Bitmap image;

        private string screenshotPath;
        private string screenshotName;

        public MainWindow()
        {
            InitializeComponent();
            screenWidth = Screen.PrimaryScreen.Bounds.Width;
            screenHeight = Screen.PrimaryScreen.Bounds.Height;

            //Set MacOS Mojave Crosshair Cursor
            //mainCanvas.Cursor = new System.Windows.Input.Cursor(new MemoryStream(Properties.Resources.Crosshair));

            mainCanvas.Cursor = System.Windows.Input.Cursors.Cross;
            InitScreenshot();
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                StopApplication();
            }
        }

        private void InitScreenshot()
        {
            Bitmap bitmap = new Bitmap(screenWidth, screenHeight);
            Graphics graphics = Graphics.FromImage(bitmap);

            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

            BitmapSource bitmapSource = BitmapConversion.ToWpfBitmap(bitmap);
            this.image = bitmap;
            mainImage.Source = bitmapSource;
        }

        private Bitmap TakeCroppedScreenshot()
        {
            System.Windows.Size sourcePoints = CalculateDPI((int)Canvas.GetLeft(mainRectangle)+1, (int)Canvas.GetTop(mainRectangle)+1);
            System.Windows.Size destinationPoints = CalculateDPI((int)mainRectangle.ActualWidth, (int)mainRectangle.ActualHeight);

            Bitmap bitmap = new Bitmap(screenWidth, screenHeight);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);

            Bitmap bitmap2 = CropBitmap(bitmap, new System.Drawing.Rectangle((int)sourcePoints.Width, (int)sourcePoints.Height, (int)destinationPoints.Width, (int)destinationPoints.Height));
            return bitmap2;
        }

        private Bitmap CropBitmap(Bitmap img, System.Drawing.Rectangle cropArea)
        {
            Bitmap bmp = new Bitmap(cropArea.Width, cropArea.Height);
            using (Graphics gph = Graphics.FromImage(bmp))
            {
                gph.DrawImage(img, new System.Drawing.Rectangle(2, 2, bmp.Width, bmp.Height), cropArea, GraphicsUnit.Pixel);
            }
            return bmp;
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
            NotifyIcon notifyIcon = new NotifyIcon
            {
                Visible = true,
                Icon = SystemIcons.Information
            };

            notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
            notifyIcon.BalloonTipClosed += (sender, e) => { var thisIcon = (NotifyIcon)sender; thisIcon.Visible = false; thisIcon.Dispose(); };
            notifyIcon.ShowBalloonTip(5000, title, $"Screenshot is saved to {fileName}", ToolTipIcon.None);
        }

        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            var thisIcon = (NotifyIcon)sender; thisIcon.Visible = false; thisIcon.Dispose();
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
                    Canvas.SetLeft(mainRectangleBorder, eventX);
                }
                else
                {
                    Canvas.SetLeft(mainRectangle, initialCanvasX);
                    Canvas.SetLeft(mainRectangleBorder, initialCanvasX);

                }

                if (eventY < initialCanvasY)
                {
                    Canvas.SetTop(mainRectangle, eventY);
                    Canvas.SetTop(mainRectangleBorder, eventY);
                }
                else
                {
                    Canvas.SetTop(mainRectangle, initialCanvasY);
                    Canvas.SetTop(mainRectangleBorder, initialCanvasY);
                }


                mainRectangle.Width = Math.Abs(initialCanvasX - e.GetPosition(mainCanvas).X);
                mainRectangle.Height = Math.Abs(initialCanvasY - e.GetPosition(mainCanvas).Y);

                mainRectangleBorder.Width = Math.Abs(initialCanvasX - e.GetPosition(mainCanvas).X);
                mainRectangleBorder.Height = Math.Abs(initialCanvasY - e.GetPosition(mainCanvas).Y);


                // Dark Area

                // Top
                topRectangle.Width = screenWidth;
                if (eventY < initialCanvasY)
                {
                    topRectangle.Height = Math.Abs(mainRectangle.Height - initialCanvasY);
                }
                else
                {
                    topRectangle.Height = initialCanvasY;
                }

                // Left
                Canvas.SetTop(leftRectangle, topRectangle.Height);
                leftRectangle.Height = screenHeight - topRectangle.Height;
                if (eventX < initialCanvasX)
                {
                    leftRectangle.Width = Math.Abs(mainRectangle.Width - initialCanvasX);
                }
                else
                {
                    leftRectangle.Width = initialCanvasX;
                }

                // Bottom
                Canvas.SetLeft(bottomRectangle, leftRectangle.Width);
                bottomRectangle.Width = screenWidth - leftRectangle.Width;
                bottomRectangle.Height = Math.Abs(screenHeight - (initialCanvasY + mainRectangle.Height));
                if (eventY < initialCanvasY)
                {
                    Canvas.SetTop(bottomRectangle, initialCanvasY);
                    bottomRectangle.Height = screenHeight - initialCanvasY;
                }
                else
                {
                    Canvas.SetTop(bottomRectangle, initialCanvasY + mainRectangle.Height);
                }

                // Right
                Canvas.SetTop(rightRectangle, 0);
                Canvas.SetLeft(rightRectangle, (initialCanvasX + mainRectangle.Width));
                rightRectangle.Width = Math.Abs(screenWidth - (initialCanvasX + mainRectangle.Width));
                rightRectangle.Height = screenHeight - bottomRectangle.Height;

                if (eventX < initialCanvasX)
                {
                    Canvas.SetLeft(rightRectangle, initialCanvasX);
                    rightRectangle.Width = screenWidth - initialCanvasX;
                }
                else
                {

                }

                topRectangle.Width = Canvas.GetLeft(rightRectangle);


                // Resolution Label Handling
                System.Windows.Size actualCoordinates = CalculateDPI((int)mainRectangle.Width, (int)mainRectangle.Height);
                mainResolution.Content = (int)actualCoordinates.Width + " x " + (int)actualCoordinates.Height;
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
                mainMenuBorder.Width = screenshotButton.Width * mainMenu.Children.Count;
                mainMenuBorder.Height = mainMenuBorder.Height + screenshotButton.Height / 2;

                // Middle
                //Canvas.SetLeft(mainMenuBorder, mainWindow.Width / 2 - mainMenuBorder.Width / 2);
                //Canvas.SetTop(mainMenuBorder, mainWindow.Height * 0.85);

                // Corner
                Canvas.SetLeft(mainMenuBorder, (initialCanvasX + mainRectangle.Width) - mainMenuBorder.Width - 1);
                Canvas.SetTop(mainMenuBorder, (initialCanvasY + mainRectangle.Height) + 1);

                // Ink Canvas
                drawCanvas.Width = screenWidth;
                drawCanvas.Height = screenHeight;
            }
        }

        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
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

        private void DrawButton_Click(object sender, RoutedEventArgs e)
        {
            if (isDraw)
            {
                isDraw = false;
                System.Windows.Controls.Panel.SetZIndex(drawCanvas, 1);
                System.Windows.Controls.Panel.SetZIndex(mainRectangle, 3);
                drawCanvas.IsEnabled = false;
            }
            else
            {
                isDraw = true;
                System.Windows.Controls.Panel.SetZIndex(drawCanvas, 3);
                System.Windows.Controls.Panel.SetZIndex(mainRectangle, 1);
                drawCanvas.IsEnabled = true;
            }
        }

        private void ScreenshotButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SetElementsVisibilityOnScreenshot(Visibility.Hidden);
        }

        private void ScreenshotButton_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            SetElementsVisibilityOnScreenshot(Visibility.Visible);
        }

        private void SetElementsVisibilityOnScreenshot(Visibility visibility)
        {
            mainResolution.Visibility = visibility;
            mainRectangleBorder.Visibility = visibility;
            mainRectangle.Visibility = visibility;
        }

        private void StopApplication()
        {
            mainWindow.Close();
        }
    }
}
