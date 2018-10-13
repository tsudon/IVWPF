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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using IVWIN;
using Path = System.IO.Path;

namespace IVWPF
{
    /// <summary>
    /// MainWindow ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private void SwitchWindow()

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

        private void MoveFilerListBox(int step)
        {
            int i = FilerListBox.SelectedIndex + step;
            if (i > FilerListBox.Items.Count - 1) i = FilerListBox.Items.Count - 1;
            if (i < 0) i = 0;
            FilerListBox.SelectedIndex = i;
            FilerListBox.ScrollIntoView(FilerListBox.SelectedItem);

        }


        private void SetFilerListBox(int pos)
        {
            int i = pos;
            if (i > FilerListBox.Items.Count - 1) i = FilerListBox.Items.Count - 1;
            if (i < 0) i = 0;
            FilerListBox.SelectedIndex = i;
            FilerListBox.ScrollIntoView(FilerListBox.SelectedItem);

        }

        private void ImageMouseDownMethod(Point point)
        {
            double mouseX = point.X;
            double mouseY = point.Y;

            double w = ImageGrid.ActualWidth;
            double h = ImageGrid.ActualHeight;

            double left = w * 0.2;
            double right = w * 0.8;
            double top = h * 0.2;
            double down = h * 0.8;

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
                    if (loadOption.isMangaMode)
                    {
                        loadOption.isMangaMode = false;
                        LoadPicture(loadOption.CurrentFile);
                    }
                    break;
                case 1:
                    FilerMode();
                    break;
                case 2:
                    if (!loadOption.isMangaMode)
                    {
                        loadOption.isMangaMode = true;
                        LoadPicture(loadOption.CurrentFile);
                    }
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
                    loader.NextFolderPicture();
                    break;
                case 7:
                    break;
                case 8:
                    loader.PreviousFolderPicture();
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
            LoadPicture(path);
        }

        private void ImageMode()
        {
            MainTab.SelectedIndex = (int)TabList.ImageTab;
        }

        private void SelectFilerListBox()
        {
            int i = FilerListBox.SelectedIndex;
            if (i < 0) return;
            string path = filer.GetSelectedPath(i);
            LogWriter.write($"{path} {loadOption.CurrentFolder}");
            if (File.Exists(path))
            {
                loadOption.CurrentFolder = Path.GetDirectoryName(path);
                loadOption.CurrentFile = Path.GetFileName(path);
                ImageMode(path);
            }
            else
            {
                if (loadOption.CurrentFile != null)
                {
                    loadOption.CurrentFolder = Path.GetDirectoryName(loadOption.CurrentFile);
                    loadOption.CurrentFile = null;
                    filer.Open(path);
                }
                else
                {
                    filer.Open(path);
                }
            }
        }

        private void ReSortFile(int i)
        {
            if (filer == null) return;
            switch (i)
            {
                case 0:
                    filer.Open(FileSortOption.SORT_BY_NAME_LOGICAL);
                    break;
                case 1:
                    filer.Open(FileSortOption.SORT_BY_NAME_LOGICAL_DESC);
                    break;
                case 2:
                    filer.Open(FileSortOption.SORT_BY_DATE);
                    break;
                case 3:
                    filer.Open(FileSortOption.SORT_BY_DATE_DESC);
                    break;
                case 4:
                    filer.Open(FileSortOption.SORT_BY_SIZE);
                    break;
                case 5:
                    filer.Open(FileSortOption.SORT_BY_SIZE_DESC);
                    break;
                default:
                    break;
            }

        }

        private void  LoadPicture(string path){
           SetWindowTitle(path);
           loader.Load(path);
        }

        public void SetWindowTitle(string path) {
            try
            {
                string name = new FileInfo(path).Name;
                this.Title = $"{loadOption.GetApplicationName()} {loadOption.GetVersion()} - {name}";
            }
            catch
            {
                this.Title = $"{loadOption.GetApplicationName()}";
            }
        }

        public void LodingStart()
        {
        }

        public void LoadingEnd()
        {
        }


    }
}
