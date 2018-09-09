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


        public Loader(String imagePath)
        {

            loadOption = new LoadOption();
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
            return PaintPicture(manager.GetImagePath(imagePath));
        }

        public bool PaintPicture(string imagePath)
        {
            try
            {
                LogWritter.write("Load" +imagePath);
                Uri uri = new Uri(imagePath);

                BitmapDecoder decoder = BitmapDecoder.Create(uri, BitmapCreateOptions.None, BitmapCacheOption.Default);

                frameCount = decoder.Frames.Count;
                framePos = 0;
                bitmapFrames = decoder.Frames;

                Clear();
                bmp = decoder.Frames[0];
                if (frameCount > 1 && loadOption.isAnimate)
                {
                    if(decoder.Metadata.Format== "gif") PaintAnimationGIF(decoder);
                }
                else {
                    isAnimation = false;
                    RePaintPicture(bmp);
                    Image.Source = bmp;
                }


            }
            catch (Exception e)
            {
                LogWritter.write(e.Message);
                return false;
            }
            return true;
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

        public void NextPiture()
        {
            bool t = false;
            if (isAnimation)
            {
                Image.BeginAnimation(Image.SourceProperty, null);
                isAnimation = false;
            } else if (frameCount > 1)
            {
                framePos++;
                bmp = bitmapFrames[framePos];
                RePaintPicture(bmp);
                Image.Source = bmp;
                return;
            }
            string imagePath = manager.GetNextPath();
            LogWritter.write(imagePath);
            if (imagePath == null) return;
            t = PaintPicture(imagePath);

        }


        public void PreviousPiture()
        {
            bool t = false;
            if (isAnimation)
            {
                Image.BeginAnimation(Image.SourceProperty, null);
                isAnimation = false;
            }
            else if (frameCount > 1)
            { 
                framePos--;
                bmp = bitmapFrames[framePos];
                RePaintPicture(bmp);
                Image.Source = bmp;
                return;
            }
            string imagePath = manager.GetPreviousPath();

            if (imagePath == null) return;
            PaintPicture(imagePath);
            if (frameCount > 1 && isAnimation)
            {
                framePos = frameCount - 1;
                bmp = bitmapFrames[framePos];
                RePaintPicture(bmp);
                Image.Source = bmp;
            }
        }

    }
}
