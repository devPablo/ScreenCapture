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
                mainWindow.Close();
            }

            if (e.Key == Key.Space)
            {
                TakeCroppedScreenshot();
                mainWindow.Close();
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

        private void TakeCroppedScreenshot()
        {
            System.Windows.Size sourcePoints = CalculateDPI((int)initialCanvasX, (int)initialCanvasY);
            System.Windows.Size destinationPoints = CalculateDPI((int) mainRectangle.ActualWidth, (int) mainRectangle.ActualHeight);

            Bitmap bitmap = CropBitmap(image, new System.Drawing.Rectangle((int) sourcePoints.Width, (int) sourcePoints.Height, (int) destinationPoints.Width, (int) destinationPoints.Height));
            bitmap.Save("C:\\Users\\Pablo\\Desktop\\screenshot.png");
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
