using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IVWIN;

namespace IVWPF
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Loader loader;
        private double startX, startY;
        private bool isMove = false;


        public MainWindow()
        {
            InitializeComponent();
            AddInitialize(null);
        }

        public MainWindow(string imagePath)
        {
            InitializeComponent();
            AddInitialize(imagePath);
        }

        private void AddInitialize(string imagePath)
        {
            loader = new Loader(imagePath);
            loader.SetImage(IVWImage);
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            loader.RePaintPicture();
        }


        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta >= 120)
            {
                loader.PreviousPiture();
            }
            else if (e.Delta <= -120)
            {
                loader.NextPiture();
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMove = true;
            Point p = e.GetPosition(this);
            startX = p.X;
            startY = p.Y;
        }

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMove)
            {
                Point p = e.GetPosition(this);
                double x = p.X - startX;
                double y = p.Y - startY;
                if (x == 0 && y == 0) { isMove = false; return; }
                loader.MovePicture(x, y);
                startX = x;
                startY = y;
            }
            isMove = false;
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                }
            }
        }

        private void Window_DragEnter(object sender, DragEventArgs e)
        {

        }


        private void Window_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string[] fileName =
                (string[])e.Data.GetData(DataFormats.FileDrop, false);
         LogWritter.write(fileName[0]);
         loader.Load(fileName[0]);

        }

         private void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            this.AllowDrop = true;
        }

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Util.CalcDpi(this);
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            
            if (isMove && e.LeftButton == MouseButtonState.Pressed )
            {
                Point p = e.GetPosition(this);
                loader.MovePicture((int)(p.X - startX), (int)(p.Y - startY));
                startX = p.X;
                startY = p.Y;
            }
        }
    }
}
