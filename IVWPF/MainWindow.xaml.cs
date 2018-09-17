using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
        LoadOption loadOption;
#pragma warning disable CS0169 // フィールド 'MainWindow.filer' は使用されていません。
        Filer filer;
#pragma warning restore CS0169 // フィールド 'MainWindow.filer' は使用されていません。

        enum TabList
        {
            ImageTab =0,
            FilerTab=1,
            SettingTab =2
        }

        public MainWindow()
        {
            InitializeComponent();
            AddInitialize();
        }

        private void AddInitialize()
        {



            string path = Loader.args.Length >= 1 ? Loader.args[0] : null;
                        if (loadOption ==null)loadOption = new LoadOption();
            loader = new Loader(path, IVWImage,loadOption);
            filer = new Filer(this,loadOption);
            this.Title = loadOption.GetApplicationName();
            FolderLabel.Content = loadOption.CurrentFolder;
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

        //誤動作を引き起こすのでとりあえずオフ
        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            if (e.StylusDevice != null)
            {
                LogWriter.write("Tapped");
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
//                WindowModeSwitch();
            }

        }

        private void WindowModeSwitch()
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

        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Util.CalcDpi(this);
        }

        private void MainCanvas_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ManipulationDelta delta = e.DeltaManipulation;

            loader.ResizePicture(delta.Scale.X,delta.Scale.Y);
                        

        }

        private void MainCanvas_TouchDown(object sender, TouchEventArgs e)
        {
            TouchPoint p = e.GetTouchPoint(this);
            LogWriter.write(" " + p.Position.X + "," + p.Position.Y);
        }

        private void MainCanvas_TouchMove(object sender, TouchEventArgs e)
        {
            TouchPoint p = e.GetTouchPoint(this);
            LogWriter.write(" " + p.Position.X + "," + p.Position.Y);

        }

        private void MainCanvas_TouchUp(object sender, TouchEventArgs e)
        {
            TouchPoint p = e.GetTouchPoint(this);
            LogWriter.write(" " + p.Position.X + "," + p.Position.Y);
        }

        bool pressAlt = false;
        bool pressShift = false;
        bool pressCtrl = false;

        DebugWindow debug = null;


        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            switch (e.Key)
            {
                case Key.System:
                case Key.LeftAlt:
                case Key.RightAlt:
                    pressAlt = true;
                    break;
                case Key.LeftShift:
                case Key.RightShift:
                    pressShift = true;
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    pressCtrl = true;
                    break;
                case Key.Left:
                    loader.NextPiture();
                    break;
                case Key.Right:
                    loader.PreviousPiture();
                    break;
                case Key.Space:
                    loader.NextPiture();
                    break;
                case Key.Escape:
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
                    break;
                case Key.D1:
                    loader.SetMangaMode(false);
                    break;
                case Key.D2:
                    loader.SetMangaMode(true);
                    break;
                case Key.D:
                    if (debug == null || !debug.IsVisible)
                    {
                        debug = new DebugWindow();
//                        LogWriter.textBox = debug.Logger;
                        debug.Show();
                    }
                    break;
                case Key.S:
                    loader.OptionSave();
                    break;
                case Key.L:
                    loader.OptionLoad();
                    break;

                case Key.Home:
                    loader.JumpPicture(0);
                    break;
                case Key.End:
                    loader.JumpPicture(-1);
                    break;
                case Key.Up:
//                    loader.PreviousFolderPicture(); // bug
                    break;
                case Key.Down:
//                    loader.NextFolderPicture();
                    break;
                default:
                    String str = "";
                    if (pressCtrl) str += "CTRL+";
                    if (pressAlt) str += "ALT+";
                    if (pressShift) str += "SHIFT+"; 
                    LogWriter.write(str + key.ToString() );
                    break;
            }

        }

        private void Main_KeyUp(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            switch (e.Key)
            {
                case Key.System:
                case Key.LeftAlt:
                case Key.RightAlt:
                    pressAlt = false;
                    break;
                case Key.LeftShift:
                case Key.RightShift:
                    pressShift = false;
                    break;
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    pressCtrl = false;
                    break;
                default:
                    break;
            }

        }

        private void WindowMain_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            loader.OptionSave();
            if (debug != null)
            {
                debug.Close();
            }
        }




        private void ImageMouseDownMethod(MouseButtonEventArgs e)
        {
            double mouseX = e.GetPosition(this).X;
            double mouseY = e.GetPosition(this).Y;

            double w = this.Width;
            double h = this.Height;

            double left = w * 0.25;
            double right = w * 0.75;
            double top = h * 0.25;
            double down = h * 0.75;

            int p = -1;

            if (mouseX < left)      // left
            {
                p = 0;
            }
            else if (mouseX > right)   //right
            {
                p = 2;
            }
            else
            {
                p = 1;      //center
            }
           if (mouseY < top)      // top
            {
                p += 0;
            }
            else if (mouseY > down)   //down
            {
                p += 6;
            }
            else
            {
                p += 3;      //center
            }

            //   0  1  2
            //   3  4  5
            //   6  7  8

            switch (p)
            {
                case 0:
                    break;
                case 1:
                    FilerMode();
                    break;
                case 2:
                    break;
                case 3:
                    loader.NextPiture();
                    break;
                case 4:
                    break;
                case 5:
                    loader.PreviousPiture();
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    break;
            }
        }

        private void FilerMode()
        {
            MainTab.SelectedIndex = (int)TabList.FilerTab;
            LogWriter.write("Switch Filer");
            filer.Open();
        }

        private void ImageMode(string path)
        {
            MainTab.SelectedIndex = (int)TabList.ImageTab;
            LogWriter.write("Switch Viewer");
            loader.Load(path);
        }

        private void ImageMode()
        {
            MainTab.SelectedIndex = (int)TabList.ImageTab;
        }

        private void WindowMain_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (MainTab.SelectedIndex) {
                case 0:
                    ImageMouseDownMethod(e);
                    break;
                case 1:
                    //MainTab.SelectedIndex = 0;
                    break;
            }
        }




        private void Image_Drop(object sender, DragEventArgs e)
        {
            string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            loader.Load(fileName[0]);
        }

        private void ImageGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            this.AllowDrop = true;
            ImageGrid.AllowDrop = true;
        }

        private void ImageGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void BuckImageButton_Click(object sender, RoutedEventArgs e)
        {
            MainTab.SelectedIndex = 0;
        }

        
        private void FilerListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            int i = FilerListBox.SelectedIndex;
            if (i < 0) return;
            string path = filer.GetSelectedPath(i);
            if(File.Exists(path))
            {
                ImageMode(path);
            } else { 
                filer.Open(path);
            }
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
