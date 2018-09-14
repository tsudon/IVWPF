using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using IVWIN;
using System.Windows.Media.Animation;

namespace IVWPF

{
    /*Loaderの本体 フォームから分離する */


    public class BitmapCache
    {
        private int CacheSize,NextCacheSize,PreviousCacheSize,Loaded,PreLoaded;
        private int offset = 0;
        string[] pathes;
        private BitmapDecoder[] sources;

        public BitmapCache(int nextCacheSize,int previousCacheSize)
        {
            NextCacheSize = nextCacheSize;
            PreviousCacheSize = previousCacheSize;
            CacheSize = nextCacheSize + previousCacheSize + 1;
            sources = new BitmapDecoder[CacheSize];
            pathes = new string[CacheSize];
            Loaded = 0;
            PreLoaded = 0;
        }

        public BitmapDecoder GetNextCache()
        {
            int i = offset;
            offset++;
            if (offset >= CacheSize - 1) offset = 0;
            if (sources[offset] == null || Loaded == 0)
            {
                offset = i;
                return null;
            }
            if (Loaded > 0)
            {
                Loaded--;
                PreLoaded++;
                if (PreLoaded > PreviousCacheSize) PreLoaded = PreviousCacheSize;
            }
            return sources[offset];
        }

        public BitmapDecoder GetPreviousCache()
        {
            int i = offset;
            offset--;
            if (offset < 0) offset = CacheSize;
            if (sources[offset] == null || PreLoaded ==0)
            {
                offset = i;
                return null;
            }
            if (PreLoaded > 0)
            {
                PreLoaded--;
                if (Loaded < NextCacheSize) Loaded++;
            }
            return sources[offset];
        }

        public BitmapDecoder GetCache(string path)
        {
            int i = 0;
            foreach (string p in pathes)
            {
                if(p == path)
                {
                    int delta = offset - i;
                    if (delta < 0) delta += CacheSize;

                    Loaded -= delta;
                    if (Loaded < 0) Loaded = 0;
                    if (Loaded > NextCacheSize) Loaded = NextCacheSize;

                    offset = i;
                    return sources[i];
                }
                i++;
            }
            return null;
        }


        public void AddNextCache(BitmapDecoder decoder)
        {
            if (decoder == null) return;
            if (Loaded >= NextCacheSize) return;
            sources[offset + Loaded] = decoder;
            Loaded++;
        }

        public void Clear()
        {
            for(int i=0; i < sources.Length; i++)
            {
                sources[i] = null;
                pathes[i] = null;
                Loaded = 0;
                PreLoaded = 0;
            }

        }
    }



    class Loader
    {
        private double offsetX = 0, offsetY = 0;
        public LoadOption loadOption;                    //image browser option
        private BitmapSource bmp; 
        public Image Image{ get; private set;}
        private int frameCount, framePos = 0;
        private ReadOnlyCollection<BitmapFrame> bitmapFrames;
        private bool isAnimation = false;
        double scaleX = 1.0, scaleY = 1.0;
        private FileManager manager;
        private BitmapCache cache;
        private int nextCacheSize = 4;
        private int previousCacheSize = 2;
        private int preLoaded = 0;
        private string currentPath;

        public Loader(String imagePath)
        {

            loadOption = new LoadOption();
            cache = new BitmapCache(nextCacheSize, previousCacheSize);

            if (imagePath != null)
            {
                Load(imagePath);
            }
        }

        public void SetImage(Image image)
        {
            this.Image = image;
        }

        public bool Load(string imagePath)
        {
            manager = new FileManager(imagePath, loadOption);
            preLoaded = 0;
            bool ret = PaintPicture(manager.GetImagePath(imagePath));
            for (int i = 0; i < nextCacheSize; i++)
            {
                GetCachePicture(manager.GetNextPath(false));
            }
            return ret;
        }


        public bool GetCachePicture(string imagePath)
        {
            if (preLoaded > nextCacheSize || imagePath == null) return false;
            try
            {
                Uri uri = new Uri(imagePath);
                BitmapDecoder decoder = BitmapDecoder.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.Default);
                cache.AddNextCache(decoder);
                bitmapFrames = decoder.Frames;
            }
            catch (Exception e)
            {
                LogWritter.write(e.ToString());
                return false;
            }
            preLoaded ++;
            return true;
        }


        private BitmapDecoder GetDecoderFromPath(string path)
        {
            try
            {
                if (path == null) return null;
                BitmapDecoder decoder = cache.GetCache(path);
                if (decoder == null)
                {
                    Uri uri = new Uri(path);
                    decoder = BitmapDecoder.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.Default);
                    cache.AddNextCache(decoder);
                }
                return decoder;
            } catch (Exception e)
            {
                LogWritter.write(e.ToString());
                return null;
            }
        }

        private bool isManga = false;

        public bool PaintPicture(string imagePath)
        {
            try
            {
                BitmapDecoder decoder = GetDecoderFromPath(imagePath);
                bitmapFrames = decoder.Frames;
                frameCount = decoder.Frames.Count;
                framePos = 0;
                isManga = false;

                bmp = bitmapFrames[0];


                Clear();
                if (frameCount > 1 && loadOption.isAnimate)
                {
                    if (decoder.Metadata.Format == "gif") PaintAnimationGIF(decoder);
                }
                else
                {
                    isAnimation = false;
                    if (loadOption.isMangaMode == false)
                    {
                        RePaintPicture(bmp);
                        Image.Source = bmp;
                    }
                    else
                    {

                        String path = manager.GetNextPath(false);

                        BitmapDecoder decoder2 = GetDecoderFromPath(path);

                        if (decoder2 != null)
                        {

                            if (Image.MinWidth >= Image.MinHeight && bmp.PixelWidth < bmp.PixelHeight) {
                                if (decoder2.Frames.Count == 1 || loadOption.isAnimate != false)
                                {
                                    BitmapSource bmp0 = new FormatConvertedBitmap(bmp, PixelFormats.Bgra32, null, 0);
                                    BitmapSource bmp1 = new FormatConvertedBitmap(decoder2.Frames[0], PixelFormats.Bgra32, null, 0);

                                    int width = bmp0.PixelWidth + bmp1.PixelWidth;
                                    int height = (bmp0.PixelHeight > bmp1.PixelHeight) ? bmp0.PixelHeight : bmp1.PixelHeight;

                                    int offsetY0 = (height - bmp0.PixelHeight) / 2;
                                    int offsetY1 = (height - bmp1.PixelHeight) / 2;

                                    double aspectSrc = (double)width / (double)height * 0.8; //許容値
                                    double aspect = Image.MinWidth / Image.MinHeight;
                                    if (aspect > aspectSrc)
                                    {
                                        manager.GetNextPath();
                                        WriteableBitmap wbmp = new WriteableBitmap(width, height, 96.0 , 96.0, PixelFormats.Bgra32, null);
                                        byte[] buf0 = new byte[bmp0.PixelWidth * bmp0.PixelHeight * 4];
                                        byte[] buf1 = new byte[bmp1.PixelWidth * bmp1.PixelHeight * 4];
                                        bmp0.CopyPixels(buf0, bmp0.PixelWidth * 4, 0);
                                        bmp1.CopyPixels(buf1, bmp1.PixelWidth * 4, 0);
                                        wbmp.WritePixels(new Int32Rect(0,offsetY1, bmp1.PixelWidth, bmp1.PixelHeight), buf1, bmp1.PixelWidth * 4, 0);
                                        wbmp.WritePixels(new Int32Rect(bmp1.PixelWidth, offsetY0, bmp0.PixelWidth, bmp0.PixelHeight), buf0, bmp0.PixelWidth * 4, 0);
                                        wbmp.Freeze();
                                        isManga = true;
                                        bmp = wbmp;
                                    }
                                }
                            }
                        }
                        RePaintPicture(bmp);
                        Image.Source = bmp;
                    }
                }
                currentPath = imagePath;
            }
            catch (Exception e)
            {
                LogWritter.write(e.Message);
                return false;
            }
            return true;
        }

        public void SetMangaMode(bool flag)
        {
            if(loadOption.isMangaMode != flag)
            {
                loadOption.isMangaMode = flag;
                manager.SetCurrentPath(currentPath);
                PaintPicture(currentPath);
            }
        }

        public void Clear()
        {
            Image.Source = null;
            if (isAnimation)
            {
                Image.BeginAnimation(Image.SourceProperty, null);
                isAnimation = false;
            }
        }


        public void ClearAndRePaintPicture()
        {
            Clear();
            RePaintPicture(this.bmp);
        }

        public void RePaintPicture() {
            RePaintPicture(this.bmp);
        }

        public void RePaintPicture(BitmapSource bmp)
        {
            RePaintPicture(bmp,false);
        }

        struct GetScaleValue
        {
            public double scaleX;
            public double scaleY;
            public int resizeWidth;
            public int resizeHight;         

        }


        private GetScaleValue GetScale (int imgWidth, int imgHeight, int width, int height,double dpiX,double dpiY)
        {
            double scaleX = 1.0;
            double scaleY = 1.0;
            int resizeWidth = imgWidth, resizeHeight =imgHeight;
            switch (loadOption.drawMode)
            {
                case DrawMode.DEFALT:
                    if (width < imgWidth)
                    {
                        resizeWidth = width;
                        resizeHeight = (int)((double)imgHeight * ((double)width / (double)imgWidth));
                        scaleX = (double)width / (double)imgWidth;
                    }
                    if (height < resizeHeight)
                    {
                        resizeHeight = height;
                        resizeWidth = (int)((double)imgWidth * ((double)height / (double)imgHeight));
                        scaleX = (double)height / (double)imgHeight;
                    }
                    scaleY = scaleX;
                    break;
                case DrawMode.HEIGHT_MATCH:
                    if (height < imgHeight)
                    {
                        resizeHeight = height;
                        resizeWidth = (int)((double)imgWidth * ((double)height / (double)imgHeight));
                        scaleY = (double)height / (double)imgHeight;
                        scaleX = scaleY;
                    }
                    break;
                case DrawMode.FRAME_MATCH:
                    resizeWidth = width;
                    resizeHeight = height;
                    scaleX = (double)width / (double)imgWidth;
                    scaleY = (double)height / (double)imgHeight;
                    break;

                case DrawMode.WIDTH_MATCH:
                    if (width < imgWidth)
                    {
                        resizeWidth = width;
                        resizeHeight = (int)((double)imgHeight * ((double)width / (double)imgWidth));
                        scaleX = (double)width / (double)imgWidth;
                        scaleY = scaleX;
                    }
                    break;

                case DrawMode.ORIGINAL: // do nothing
                    break;
            }

            double dpix = (bmp.DpiX == 0 )? 96.0 : bmp.DpiX;    //bitmap Dpiが 0の時の対策
            double dpiy = (bmp.DpiY == 0) ? 96.0 : bmp.DpiY;


            scaleX *= dpix / Util.GetDpiX();
            scaleY *= dpiy / Util.GetDpiY();


            GetScaleValue value = new GetScaleValue();
            value.scaleX = scaleX;
            value.scaleY = scaleY;
            value.resizeHight = resizeHeight;
            value.resizeWidth = resizeWidth;

            return value;
        }



        public void PaintAnimationGIF(BitmapDecoder decoder)
        {
            frameCount = decoder.Frames.Count;
            framePos = 0;
            bitmapFrames = decoder.Frames;

            bmp = decoder.Frames[0];
            int width = (int)Image.MinWidth;
            int height = (int)Image.MinHeight;
            int imgWidth = bmp.PixelWidth;
            int imgHeight = bmp.PixelHeight;

            ObjectAnimationUsingKeyFrames animation = new ObjectAnimationUsingKeyFrames();
            LogWritter.write("This File is Animation GIF.");
            isAnimation = true;

            long time = 0;
            long span = 1000 * 10;

            byte[] imgBuffer = new byte[imgWidth * imgHeight];

            WriteableBitmap wbmp = new WriteableBitmap(imgWidth, imgHeight, bmp.DpiX, bmp.DpiY,bmp.Format, bmp.Palette);

            GetScaleValue v = GetScale(imgWidth, imgHeight, width, height, bmp.DpiX, bmp.DpiY);
            scaleX = v.scaleX;
            scaleY = v.scaleY;
            int resizeWidth = v.resizeWidth;
            int resizeHeight = v.resizeHight;

            if (bmp.Format != PixelFormats.Indexed8)
            {
                bmp = new FormatConvertedBitmap(bmp,PixelFormats.Indexed8,null,0);
                bmp.Freeze();
            }
 
            byte[] buf = new byte[imgWidth * imgHeight];
            bmp.CopyPixels(buf, imgWidth, 0);
            wbmp.WritePixels(new Int32Rect(0, 0, imgWidth, imgHeight), buf, imgWidth, 0);

            for (int i = 0; i < frameCount; i++)
            {
                BitmapSource fbmp = bitmapFrames[i];
                int transpearentColor = 0;
                BitmapMetadata metadata = fbmp.Metadata as BitmapMetadata;
                bool transpearent = false, hasLocalPalette = false;
                int delay = 0,startX=0,startY=0,w=imgWidth,h=imgHeight;

                BitmapPalette palette = bmp.Palette ;

                if (metadata != null)
                {
                    try
                    {
                        startX = (UInt16)metadata.GetQuery("/imgdesc/Left");
                        startY = (UInt16)metadata.GetQuery("/imgdesc/Top");
                        w = (UInt16)metadata.GetQuery("/imgdesc/Width");
                        h = (UInt16)metadata.GetQuery("/imgdesc/Height");
                        delay = (UInt16)metadata.GetQuery("/grctlext/Delay") * 10;
                        transpearent = (Boolean)metadata.GetQuery("/grctlext/TransparencyFlag");
                        if (transpearent) transpearentColor = (Byte)metadata.GetQuery("/grctlext/TransparentColorIndex");
                        hasLocalPalette = (Boolean)metadata.GetQuery("/imgdesc/LocalColorTableFlag");
                        if (hasLocalPalette)
                        {
                            int size = (byte)metadata.GetQuery("/imgdesc/LocalColorTableSize");
                            palette = fbmp.Palette;
                        }

                    }
                    catch
                    {
                        LogWritter.write("Query Error");
                        //no data
                    }
                }


                if (fbmp.Format != PixelFormats.Indexed8)
                {
                    fbmp = new FormatConvertedBitmap(fbmp,PixelFormats.Indexed8,null,0);
                    fbmp.Freeze();
                }

                byte[] fbuf = new byte[w*h];
                fbmp.CopyPixels(fbuf, w, 0);

                if (transpearent)
                {
                     for (int y = 0; y < h; y++)
                      {
                            int srcOffset = y * w;
                            int destOffset = (startY + y) * imgWidth + startX;
                            for (int x = 0; x < w; x++)
                            {
                                if (transpearentColor != fbuf[srcOffset + x])
                                {
                                    buf[destOffset + x] = fbuf[srcOffset + x];
                                }
                            }

                     }
                }
                else
                {
                    for (int y = 0; y < h; y++)
                    {
                        int srcOffset = y * w;
                        int destOffset = (y + startY) * imgWidth + startX;
                        Array.Copy(fbuf,srcOffset, buf,destOffset, w);
                    }

                }


                WriteableBitmap wfbmp = new WriteableBitmap(imgWidth, imgHeight, bmp.DpiX, bmp.DpiY, bmp.Format, palette);
                wfbmp.WritePixels(new Int32Rect(0, 0, imgWidth, imgHeight), buf, imgWidth, 0);

                DiscreteObjectKeyFrame key = new DiscreteObjectKeyFrame();
                key.KeyTime = new TimeSpan(time);
//                key.Value = bitmapFrames[i];
                key.Value = wfbmp;
                animation.KeyFrames.Add(key);
                time += delay * span;
            }
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.Duration = new TimeSpan(time);

            offsetX = (width - resizeWidth) / 2.0;
            offsetY = (height - resizeHeight) / 2.0;

            TransformGroup transforms = new TransformGroup();
            transforms.Children.Add(new ScaleTransform(scaleX, scaleY));
            Matrix matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);
            transforms.Children.Add(new MatrixTransform(matrix));
            Image.RenderTransform = transforms;

            Image.Source = bmp;
            Image.BeginAnimation(Image.SourceProperty, animation);
        }


        public void RePaintPicture(BitmapSource bmp, bool isAnimation)
        {
            if (isAnimation) return;
            if (bmp == null) return;
            int width = (int)Image.MinWidth;
            int height = (int)Image.MinHeight;
            int imgWidth = bmp.PixelWidth;
            int imgHeight = bmp.PixelHeight;



            GetScaleValue v = GetScale(imgWidth, imgHeight, width, height, bmp.DpiX, bmp.DpiY);
            scaleX = v.scaleX;
            scaleY = v.scaleY;
            int resizeWidth = v.resizeWidth;
            int resizeHeight = v.resizeHight;


            offsetX = (width - resizeWidth) / 2.0;
            offsetY = (height - resizeHeight) / 2.0;


            TransformGroup transforms = new TransformGroup();
            transforms.Children.Add(new ScaleTransform(scaleX, scaleY));
            Matrix matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);
            transforms.Children.Add(new MatrixTransform(matrix));
            Image.RenderTransform = transforms;


            //           if (image == null) LogWritter.write("Image Objects is null");
        }

        public void MovePicture(double x, double y)
        {
            offsetX += x;
            offsetY += y;
            // アフィン変換による平行移動
            //
            //  | 1 0 x | |dx|  x' = x+dx
            //  | 0 1 y | |dy|  x' = y+dy
            //  | 0 0 1 | | 1|
            TransformGroup transforms = new TransformGroup();
            transforms.Children.Add(new ScaleTransform(scaleX, scaleY));
            Matrix matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);
            transforms.Children.Add(new MatrixTransform(matrix));
            Image.RenderTransform = transforms;
        }

        public void ResizePicture(double deltaX, double deltaY)
        {

            scaleX *= deltaX;
            scaleY *= deltaY;

            TransformGroup transforms = new TransformGroup();
            transforms.Children.Add(new ScaleTransform(scaleX, scaleY));
            Matrix matrix = new Matrix(1, 0, 0, 1, offsetX, offsetY);
            transforms.Children.Add(new MatrixTransform(matrix));
            Image.RenderTransform = transforms;
        }


        public void NextPiture()
        {
            try
            {
                if (isManga)
                {
                    isManga = false;
                }
                bool t = isAnimation;
                if (frameCount > 1 && framePos < frameCount && !isAnimation)
                {
                    framePos++;
                    bmp = bitmapFrames[framePos];
                    RePaintPicture(bmp);
                    Image.Source = bmp;
                    return;
                }
                string imagePath = manager.GetNextPath();
                if (imagePath == null) return;
                t = PaintPicture(imagePath);
                if (!isAnimation && t)
                {
                    Image.BeginAnimation(Image.SourceProperty, null);
                    isAnimation = false;
                    frameCount = 0;
                }
                if( ! GetCachePicture(manager.GetNextPath(false))) preLoaded --;
            }
            catch(Exception e)
            {
                LogWritter.write(e.ToString());
            }
        }

        public void PreviousPiture()
        {
            try
            {
                if (isManga)
                {
                    manager.GetPreviousPath();

                    isManga = false;
                }
                bool t = isAnimation;
                string imagePath = manager.GetPreviousPath();
                if (imagePath == null) return;

                PaintPicture(imagePath);

                if (!isAnimation && t)
                {
                    Image.BeginAnimation(Image.SourceProperty, null);
                    isAnimation = false;
                }

                if (frameCount > 1 && !isAnimation)
                {
                    framePos = frameCount - 1;
                    bmp = bitmapFrames[framePos];
                    RePaintPicture(bmp);
                    Image.Source = bmp;
                }
            }
            catch (Exception e)
            {
                LogWritter.write(e.ToString());
            }

        }

        public void JumpPicture(int pos)
        {

            try
            {
                if (isManga)
                {
                    isManga = false;
                }
                if (!isAnimation)
                {
                    Image.BeginAnimation(Image.SourceProperty, null);
                    isAnimation = false;
                }

                cache.Clear();
                String path = manager.GetPathFormPos(pos);
                PaintPicture(path);
            }
            catch (Exception e)
            {
                LogWritter.write(e.ToString());
            }

        }

        public void NextFolderPicture()
        {
            try
            {
                if (isManga)
                {
                    isManga = false;
                }
                if (!isAnimation)
                {
                    Image.BeginAnimation(Image.SourceProperty, null);
                    isAnimation = false;
                }

                cache.Clear();
                String path = manager.GetNextFolderFile();
                PaintPicture(path);
            }
            catch (Exception e)
            {
                LogWritter.write(e.ToString());
            }


        }

        public void PreviousFolderPicture()
        {
            try
            {
                if (isManga)
                {
                    isManga = false;
                }
                if (!isAnimation)
                {
                    Image.BeginAnimation(Image.SourceProperty, null);
                    isAnimation = false;
                }

                cache.Clear();
                String path = manager.GetPreviousFolderFile();
                PaintPicture(path);
            }
            catch (Exception e)
            {
                LogWritter.write(e.ToString());
            }


        }

    }
}
