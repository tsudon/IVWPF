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
    /// イベントロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        Loader loader;
        private double startX, startY;
        private bool isMove = false;
        LoadOption loadOption;
        Filer filer;

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
//            string path = Loader.args.Length >= 1 ? Loader.args[0] : null;
            if (loadOption ==null)loadOption = new LoadOption();
            //            loader = new Loader(path, IVWImage,loadOption);
            loader = new Loader(IVWImage,loadOption);
            loader.SetTitleCallback = SetWindowTitle;
            loader.LoadStartCallback = LodingStart;
            loader.LoadEndCallback = LoadingEnd;

            filer = new Filer(this,loadOption);
            if (loadOption.CurrentFile != null)
            {
                loader.SetTitleCallback(loadOption.CurrentFile);
            }
            FolderLabel.Content = loadOption.CurrentFolder;
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            loader.RePaintPicture();
        }


        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(pressCtrl){
                if (e.Delta >= 120)
                {
                    loader.ResizePicture(1.1,1.1);
                }
                else if (e.Delta <= -120)
                {
                    loader.ResizePicture(0.9,0.9);
                }
            } else{
                if (e.Delta >= 120)
                {
                    loader.PreviousPiture();
                }
                else if (e.Delta <= -120)
                {
                    loader.NextPiture();
                }
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


        private void Main_Loaded(object sender, RoutedEventArgs e)
        {
            Util.CalcDpi(this);
        }


        Dictionary<int,TouchDevice> _touchesImageGrid = null;

        private void ImageGrid_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            //指　二本の時のみ
            if (_touchesImageGrid.Count == 2)
            {
                ManipulationDelta delta = e.DeltaManipulation;
                loader.ResizePicture(delta.Scale.X, delta.Scale.Y);
                loader.MovePicture(delta.Translation.X, delta.Translation.Y);
            }
        }

        private void ImageGrid_TouchDown(object sender, TouchEventArgs e)
        {
            if (_touchesImageGrid == null)
            {
                _touchesImageGrid = new Dictionary<int, TouchDevice>();
            }

            if(!_touchesImageGrid.ContainsKey(e.TouchDevice.Id))
            {
                _touchesImageGrid.Add(e.TouchDevice.Id, e.TouchDevice);
            }
            if(_touchesImageGrid.Count == 1)
            {
                Point p = e.GetTouchPoint(ImageGrid).Position;
                _xImageGrid = p.X;
                _yImageGrid = p.Y;
            }
        }

        private void ImageGrid_TouchMove(object sender, TouchEventArgs e)
        {
//            TouchPoint p = e.GetTouchPoint(this);
//            LogWriter.write(" " + p.Position.X + "," + p.Position.Y);

        }

        private void ImageGrid_TouchUp(object sender, TouchEventArgs e)
        {
            Point p = e.GetTouchPoint(ImageGrid).Position;
            if (_touchesImageGrid.ContainsKey(e.TouchDevice.Id))
            {
                _touchesImageGrid.Remove(e.TouchDevice.Id);
            }

            double x = p.X;
            double y = p.Y;
            double deltaX = x - _xImageGrid;
            double deltaY = y - _yImageGrid;

            LogWriter.write($"{_xImageGrid},{_yImageGrid} - {x},{y} / {deltaX},{deltaY}");

            switch (MainTab.SelectedIndex)

            {
                case 0:
                    if (Math.Abs(deltaX) < _bounds && Math.Abs(deltaY) < _bounds)
                    {
                        ImageMouseDownMethod(p);
                    }
                    else if (Math.Abs(deltaY) < _swaip)
                    {

                        if (deltaX <= -1 * _swaip) //Right Swaip

                        {
                            loader.PreviousPiture();

                        }
                        else if (deltaX >= _swaip) //Left Swaip
                        {
                            loader.NextPiture();
                        }
                    }
                    else if (Math.Abs(deltaX) < _swaip)
                    {
                        if (deltaY >= -1 * _swaip) //Up Swaip
                        {
                            //
                        }
                        else if (deltaY >= _swaip) //Down Swaip
                        {
                            //
                        }
                    }
                    break;
                case 1:
                    //MainTab.SelectedIndex = 0;
                    break;
            }
        }

        bool pressAlt = false;
        bool pressShift = false;
        bool pressCtrl = false;

#if DEBUG
        DebugWindow debug = null;
#endif

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (MainTab.SelectedIndex == (int)TabList.ImageTab)
            {
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
                        FilerMode();
                        break;
                    case Key.D1:
                        loader.SetMangaMode(false);
                        break;
                    case Key.D2:
                        loader.SetMangaMode(true);
                        break;
                    case Key.D:
#if DEBUG
                        if (debug == null || !debug.IsVisible)
                        {
                            debug = new DebugWindow();
                            LogWriter.SetTextBlock(debug.Logger);
                            debug.Show();
                        }
                        break;
#endif
                        case Key.S:

                        break;
                    case Key.L:

                        break;

                    case Key.Home:
                        loader.JumpPicture(0);
                        break;
                    case Key.End:
                        loader.JumpPicture(-1);
                        break;
                    case Key.Up:
                        loader.PreviousFolderPicture(); // bug
                        break;
                    case Key.Down:
                        loader.NextFolderPicture();
                        break;
                    default:
                        String str = "";
                        if (pressCtrl) str += "CTRL+";
                        if (pressAlt) str += "ALT+";
                        if (pressShift) str += "SHIFT+";
                        LogWriter.write(str + key.ToString());
                        break;
                }
            }
            else if (MainTab.SelectedIndex == (int)TabList.FilerTab)
            {
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
                        break;
                    case Key.Right:
                        break;
                    case Key.Space:
                        break;
                    case Key.Escape:
                        ImageMode();
                        break;
                    case Key.D1:
                        break;
                    case Key.D2:
                        break;
                    case Key.D:
                        break;
                    case Key.S:
                        break;
                    case Key.L:
                        break;
                    case Key.Home:
                        SetFilerListBox(-0);
                        break;
                    case Key.End:
                        SetFilerListBox(-1);
                        break;
                    case Key.PageDown:
                        MoveFilerListBox(10);
                        break;
                    case Key.PageUp:
                        MoveFilerListBox(-10);
                        break;
                    case Key.Up:
                        MoveFilerListBox(-1);
                        break;
                    case Key.Down:
                        MoveFilerListBox(1);
                        break;
                    case Key.Enter:
                        SelectFilerListBox();
                        break;
                    default:
                        String str = "";
                        if (pressCtrl) str += "CTRL+";
                        if (pressAlt) str += "ALT+";
                        if (pressShift) str += "SHIFT+";
                        LogWriter.write(str + key.ToString());
                        break;
                }

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
#if DEBUG
            if (debug != null)
            {
                debug.Close();
            }
#endif
        }


        private void Image_Drop(object sender, DragEventArgs e)
        {
            string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
           LoadPicture(fileName[0]);
        }

        private void ImageGrid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.All;
            this.AllowDrop = true;
            ImageGrid.AllowDrop = true;
        }

        private void BuckImageButton_Click(object sender, RoutedEventArgs e)
        {
            ImageMode();
        }


        private void FilerListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectFilerListBox();
        }


        private void WindowMain_TouchDown(object sender, TouchEventArgs e)
        {



        }

        private void WindowMain_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        double _bounds = 20.0,_swaip =50.0;
        double _xImageGrid, _yImageGrid;

        private void ImageGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {

            Point p = e.MouseDevice.GetPosition(ImageGrid);
            _xImageGrid = p.X;
            _yImageGrid = p.Y;
        }

        private void ImageGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {

            Point p = e.MouseDevice.GetPosition(ImageGrid);

            double x = p.X;
            double y = p.Y;
            double deltaX = x - _xImageGrid;
            double deltaY = y - _yImageGrid;

            LogWriter.write($"{_xImageGrid},{_yImageGrid} - {x},{y} / {deltaX},{deltaY}");

            switch (MainTab.SelectedIndex)

            {
                case 0:
                    if (Math.Abs(deltaX) < _bounds && Math.Abs(deltaY) < _bounds)
                    {
                        ImageMouseDownMethod(p);
                    }
                    else if (Math.Abs(deltaY) < _swaip)
                    {

                        if (deltaX <= -1 * _swaip) //Right Swaip

                        {
                            loader.PreviousPiture();

                        }
                        else if (deltaX >= _swaip) //Left Swaip
                        {
                            loader.NextPiture();
                        }
                    }
                    else if (Math.Abs(deltaX) < _swaip)
                    {
                        if (deltaY >= -1 * _swaip) //Up Swaip
                        {
                            //
                        }
                        else if (deltaY >= _swaip) //Down Swaip
                        {
                            //
                        }
                    }
                    break;
                case 1:
                    //MainTab.SelectedIndex = 0;
                    break;
            }
        }




        private void SortSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = SortSelect.SelectedIndex;
            ReSortFile(i);

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
