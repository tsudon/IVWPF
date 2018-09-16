using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IVWIN
{
    public class LocalDecoder
    {
        public BitmapCodecInfo CodecInfo { get; internal set; }
        public ReadOnlyCollection<ColorContext> ColorContexts { get; internal set; }
        public ReadOnlyCollection<BitmapFrame> Frames { get; internal set; }
        public BitmapMetadata Metadata { get; internal set; }
        public BitmapPalette Palette { get; internal set; }
        public BitmapSource Preview { get; internal set; }
        public BitmapSource Thumnail { get; internal set; }
        public bool IsWPF { get; internal set; }
        public bool IsAnimation;
        public bool IsUgoira;
        public int[] Delays;


        static public LocalDecoder TransrateBitmapDecoderToLocal(BitmapDecoder decoder)
        {
            LocalDecoder local = new LocalDecoder();
            local.CodecInfo = decoder.CodecInfo;
            local.ColorContexts = decoder.ColorContexts;
            local.Frames = decoder.Frames;
            local.Metadata = decoder.Metadata;
            local.Palette = decoder.Palette;
            local.Preview = decoder.Preview;
            local.Thumnail = decoder.Thumbnail;
            local.IsWPF = true;
            local.IsUgoira = false;
            return local;
        }
    }
}