using IVWIN;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IVWPF
{
    public class Filer : IValueConverter
    {
        LoadOption loadOption;
        MainWindow window;
        List<string> listBoxList = new List<string>();

        public Filer(MainWindow window, LoadOption load)
        {
            this.window = window;
            loadOption = load;
            window.FolderLabel.Content = loadOption.CurrentFolder;
        }



        private Grid ListGrid(ImageSource source,string filename,DateTime date,long size) 
        {

            Binding bindingLabel = new Binding("ActualWidth") { Source = window.FilerListBox, Mode = BindingMode.OneWay,Converter = this };


            Grid listGrid = new Grid();
            listGrid.Margin = new Thickness(0);

            var column1 = new ColumnDefinition();
            column1.Width = new GridLength(56.0, GridUnitType.Pixel);
            listGrid.ColumnDefinitions.Add(column1);
            var column2 = new ColumnDefinition();
            column2.Width = new GridLength(0, GridUnitType.Auto);
            listGrid.ColumnDefinitions.Add(column2);


            Image icon = new Image();
            icon.Width = 56;
            icon.Height = 56;
            icon.HorizontalAlignment = HorizontalAlignment.Stretch;
            icon.VerticalAlignment = VerticalAlignment.Stretch;
            icon.Source = source;

            Grid.SetColumn(icon, 0);

            Grid labelGrid = new Grid();
            labelGrid.Margin = new Thickness(0);

            labelGrid.SetBinding(Grid.WidthProperty, bindingLabel);
            labelGrid.SetBinding(Grid.MinWidthProperty, bindingLabel);

            RowDefinition row1 = new RowDefinition();
            row1.Height = new GridLength(34.0 ,GridUnitType.Pixel);
            labelGrid.RowDefinitions.Add(row1);

            RowDefinition row2 = new RowDefinition();
            row2.Height = new GridLength(0,GridUnitType.Auto);
            labelGrid.RowDefinitions.Add(row2);

            Grid.SetColumn(labelGrid, 1);

            Label filenameLabel = new Label();
            filenameLabel.Margin = new Thickness(0);
            filenameLabel.SetBinding(Label.WidthProperty, bindingLabel);
            filenameLabel.SetBinding(Label.MinWidthProperty, bindingLabel);

            filenameLabel.FontSize = 18.0;
            filenameLabel.Content = filename;
            filenameLabel.HorizontalAlignment = HorizontalAlignment.Left;
            filenameLabel.VerticalAlignment = VerticalAlignment.Top;
            Grid.SetColumnSpan(filenameLabel, 2);
            Grid.SetRow(filenameLabel, 0);

            Label dateLabel = new Label();
            dateLabel.Margin = new Thickness(0); 
            dateLabel.FontSize = 9.0;
            dateLabel.Content = date.ToLocalTime().ToString();
            dateLabel.HorizontalAlignment = HorizontalAlignment.Left;
            Grid.SetRow(dateLabel, 1);

            Label sizeLabel = new Label();
            sizeLabel.Margin = new Thickness(50,0,0,0);
            sizeLabel.FontSize = 9.0;
            sizeLabel.HorizontalAlignment = HorizontalAlignment.Right;
            if (size < 0)
            {
                sizeLabel.Content = "";
            } else
            {
                size = (size + 1023) / 1024;
                sizeLabel.Content = $"{size:N} KB";
            }
            Grid.SetRow(sizeLabel, 1);


            labelGrid.Children.Add(filenameLabel);
            labelGrid.Children.Add(dateLabel);
            labelGrid.Children.Add(sizeLabel);

            listGrid.Children.Add(icon);
            listGrid.Children.Add(labelGrid);


            return listGrid;
        }

        internal void Open()
        {
            Open(loadOption.CurrentFolder,loadOption.sortOption);
        }

        internal void Open(string path)
        {
            Open(path,loadOption.sortOption);
        }

        internal void Open(FileSortOption option)
        {
            loadOption.sortOption = option;
            Open(loadOption.CurrentFolder, option);
        }

        internal void Open(string path,FileSortOption option)
        {
            try
            {
                BitmapImage upIcon = window.Resources["UpICON"] as BitmapImage;
                BitmapImage folderIcon = window.Resources["FolderICON"] as BitmapImage;
                BitmapImage gifIcon = window.Resources["GifICON"] as BitmapImage;
                BitmapImage jpegIcon = window.Resources["JpegICON"] as BitmapImage;
                BitmapImage pngIcon = window.Resources["PngICON"] as BitmapImage;
                BitmapImage zipIcon = window.Resources["ZipICON"] as BitmapImage;
                BitmapImage iconIcon = window.Resources["IconICON"] as BitmapImage;
                BitmapImage tiffIcon = window.Resources["TiffICON"] as BitmapImage;
                BitmapImage bmpIcon = window.Resources["BmpICON"] as BitmapImage;
                BitmapImage driveIcon = window.Resources["DriveICON"] as BitmapImage;

                System.Collections.IDictionary directory = window.Resources;

                if (path == "drive:")
                {
                    window.FolderLabel.Content = "drive:";
                    DriveList();
                    return;
                }

                string foldername = path != null ? path : loadOption.CurrentFolder;


                if (!Directory.Exists(path))
                {
                    foldername = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                }

                window.FolderLabel.Content = foldername;
                VirtualFileList list = new VirtualFileList(loadOption);
                list.ListDirectory(foldername);
                VirtualFileInfo[] dirInfos = list.dirInfos;
                VirtualFileInfo[] infos = list.infos;
                ItemCollection items = window.FilerListBox.Items;

                items.Clear();
                listBoxList.Clear();


                DateTime date = Directory.GetCreationTime(foldername);
                DirectoryInfo d = Directory.GetParent(foldername);
                Grid grid;
                if (d == null)
                {
                    grid = ListGrid(driveIcon, "drive:", date, -1);
                    items.Add(grid);
                    listBoxList.Add("drive:");
                }
                else
                {
                    grid = ListGrid(upIcon, d.Name, date, -1);
                    items.Add(grid);
                    listBoxList.Add(d.FullName);
                }

                if (((int)option & (int)FileSortOption.SORT_DESC) == 1)
                {
                    AddFile(gifIcon, jpegIcon, pngIcon, zipIcon, iconIcon, tiffIcon, bmpIcon, foldername, infos, items, grid);
                    AddFolder(folderIcon, dirInfos, items, grid);
                } else
                {
                    AddFolder(folderIcon, dirInfos, items, grid);
                    AddFile(gifIcon, jpegIcon, pngIcon, zipIcon, iconIcon, tiffIcon, bmpIcon, foldername, infos, items, grid);
                }
                window.FilerListBox.SelectedIndex = 0;
            }
            catch (Exception e)
            {
                LogWriter.write(e.ToString());
            }
        }

        private void AddFile(BitmapImage gifIcon, BitmapImage jpegIcon, BitmapImage pngIcon, BitmapImage zipIcon, BitmapImage iconIcon, BitmapImage tiffIcon, BitmapImage bmpIcon, string foldername, VirtualFileInfo[] infos, ItemCollection items, Grid grid)
        {
            foreach (var info in infos)
            {
                string ext = Path.GetExtension(info.Name).ToLower();
                BitmapImage bmp = null;
                switch (ext)
                {
                    case ".bmp":
                    case ".dib":
                    case ".rle":
                        bmp = bmpIcon;
                        break;
                    case ".ico":
                    case ".icon":
                        bmp = iconIcon;
                        break;
                    case ".gif":
                        bmp = gifIcon;
                        break;
                    case ".jpeg":
                    case ".jpe":
                    case ".jpg":
                    case ".jfif":
                    case ".exif":
                        bmp = jpegIcon;
                        break;
                    case ".png":
                        bmp = pngIcon;
                        break;
                    case ".tiff":
                    case ".tif":
                        bmp = tiffIcon;
                        break;
                    case ".zip":
                        bmp = zipIcon;
                        break;

                }

                grid = ListGrid(bmp, info.Name, info.CreationTime, info.Length);
                items.Add(grid);
                listBoxList.Add(info.FullName);
                loadOption.CurrentFolder = foldername;
            }
        }

        private void AddFolder(BitmapImage folderIcon, VirtualFileInfo[] dirInfos, ItemCollection items, Grid grid)
        {
            foreach (var info in dirInfos)
            {
                grid = ListGrid(folderIcon, info.Name, info.CreationTime, -1);
                items.Add(grid);
                listBoxList.Add(info.FullName);
            }
        }

        internal void DriveList()
        {
            BitmapImage driveIcon = window.Resources["DriveICON"] as BitmapImage;
            ItemCollection items = window.FilerListBox.Items;
            Grid grid;
            string[] drives = Directory.GetLogicalDrives();

            items.Clear();
            listBoxList.Clear();

            foreach (string drive in drives)
            {
                grid = ListGrid(driveIcon, drive, new DateTime(), -1);
                items.Add(grid);
                listBoxList.Add(drive);
            }

        }

        public string GetSelectedPath(int num)
        {
            return listBoxList.ToArray()[num];
        }

        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double v= (double) value - 80;
            v = v >= 0 ? v : 0;
            return v;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }


    }
}
