using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IVWIN
{
    public class ZipImageDecoder :LocalDecoder
    {

        public ZipImageDecoder(string path)
        {
            IsWPF = false;
            List<BitmapFrame> frameList = new List<BitmapFrame>();
            using (ZipArchive zip = ZipFile.OpenRead(path))
            {
                ReadOnlyCollection<ZipArchiveEntry> entries = zip.Entries;
                PixivAnimationList list = null;
                String jsonPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + ".json";
                if (File.Exists(jsonPath))
                {
                    list = PixivAnimationList.PixivAnimationJSON(jsonPath);
                }
                if(list != null) IsAnimation = true;

                if (IsAnimation)
                {
                    IsUgoira = true;
                    LogWriter.write("Pixiv Animation");
                    Dictionary<string, ZipArchiveEntry> dic = new Dictionary<string, ZipArchiveEntry>();
                    foreach (ZipArchiveEntry entry in entries)
                    {
                        dic.Add(entry.Name, entry);
                    }

                    Delays = new int[list.Length];

                    for (int i = 0; i < list.Length; i++)
                    {
                        ZipArchiveEntry entry = null;

                        if (dic.ContainsKey(list.Files[i])){
                            entry = dic[list.Files[i]];
                        }
                        else
                        {
                            LogWriter.write("not found " + dic[list.Files[i]]);
                        }
                        using (Stream stream = entry.Open())
                        {
                            try
                            {
                                BitmapDecoder decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                                frameList.Add(decoder.Frames[0]);
                                Delays[i] = list.Delays[i];
                            }
                            catch (Exception e)
                            {
                                LogWriter.write(e.ToString());
                            }

                        }

                        this.Frames = new ReadOnlyCollection<BitmapFrame>(frameList);
                        this.Preview = this.Frames[0];
                        this.Thumnail = Frames[0].Thumbnail;



                    }
                    return;
                }
                throw new NotSupportedException();
                /*
                else
                {
                    //no support;

                    foreach (ZipArchiveEntry entry in entries)
                    {
                        string ext = Path.GetExtension(entry.Name).ToLower();
                        Regex regex = new Regex("\\.(bmp|dib|rle|ico|icon|gif|jpeg|jpe|jpg|jfif|exif|png|tiff|tif)$");
                        if (regex.IsMatch(ext))
                        {
                            using (Stream stream = entry.Open())
                            {
                                try
                                {
                                    BitmapDecoder decoder = null;
                                    switch (ext)
                                    {
                                        case ".bmp":
                                        case ".dib":
                                        case ".rle":
                                            decoder = new BmpBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                                            break;
                                        case ".ico":
                                        case ".icon":
                                            decoder = new IconBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                                            break;
                                        case ".gif":
                                            decoder = new GifBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                                            break;
                                        case ".jpeg":
                                        case ".jpe":
                                        case ".jpg":
                                        case ".jfif":
                                        case ".exif":
                                            decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                                            break;
                                        case ".png":
                                            decoder = new PngBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                                            break;
                                        case ".tiff":
                                        case ".tif":
                                            decoder = new TiffBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.Default);
                                            break;
                                    }
                                    if (decoder != null)
                                    {
                                        for (int i = 0; i < decoder.Frames.Count; i++)
                                        {
                                            frameList.Add(decoder.Frames[i]);
                                        }
                                    }
                                }
                                catch(Exception e)
                                {
                                    LogWriter.write(e.ToString());
                                }

                            }
                            this.Frames = new ReadOnlyCollection<BitmapFrame>(frameList);
                            this.Preview = this.Frames[0];
                            this.Thumnail = Frames[0].Thumbnail;
                        }
                    }
                }
                */
            }           
        }     
    }
}
