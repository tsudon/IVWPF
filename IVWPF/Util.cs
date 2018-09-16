using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Interop;

namespace IVWIN
{
    public class PixivAnimationList
    {
        public int Length { get; internal set; }
        public string[] Files { get; internal set; }
        public int[] Delays { get; internal set; }

        static public PixivAnimationList PixivAnimationJSON(string path)
        {
            dynamic json = Util.JSONLoader(path);
            if (json == null) return null;
            PixivAnimationList list = new PixivAnimationList();
            try
            {
                if (json["application"] == "IVWINAnimation")
                {
                    double frameRate = 1 / json["animation"]["framerate"];
                    var files = json["animation"]["files"];
                    int size = files.Length;
                    list.Length = size;
                    list.Files = new string[size];
                    list.Delays = new int[size];
                    for (int i = 0; i < size; i++)
                    {
                        list.Files[i] = files[i]["f"];
                        list.Delays[i] = (int)((double)int.Parse(files[i]["d"]) * frameRate);
                    }
                }
                else
                {
                    return null;
                }
            }
#pragma warning disable CS0168 // 変数 'e' は宣言されていますが、使用されていません。
            catch (Exception e)
#pragma warning restore CS0168 // 変数 'e' は宣言されていますが、使用されていません。
            {
                try
                {
                    var files = json["info"]["path"][0]["frames"];
                    int size = files.Length;
                    list.Length = size;
                    list.Files = new string[size];
                    list.Delays = new int[size];
                    for (int i = 0; i < size; i++)
                    {
                        list.Files[i] = files[i]["f"];
                        list.Delays[i] = int.Parse(files[i]["d"].ToString());
                    }
                }
                catch (Exception e2)
                {
                    LogWriter.write(e2.ToString());
                    list = null;
                }
            }
            return list;
        }

    }

    public class Util
    {

        enum MonitorDefaultTo { Null, Primary, Nearest }
        enum MonitorDpiType { Effective, Angular, Raw, Default = Effective }

        private static double dpiX = 96.0;
        private static double dpiY = 96.0;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorDefaultTo dwFlags);
        // ディスプレイハンドルからDPIを取得

        [DllImport("SHCore.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        static extern void GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, ref uint dpiX, ref uint dpiY);

        static public void CalcDpi(Window window)
        {
            var helper = new WindowInteropHelper(window);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            var hmonitor = MonitorFromWindow(source.Handle, MonitorDefaultTo.Nearest);
            uint dpix = (uint)dpiX, dpiy = (uint)dpiY;
            GetDpiForMonitor(hmonitor, MonitorDpiType.Default, ref dpix, ref dpiy);
            dpiX = dpix;
            dpiY = dpiy;
        }

        static public double GetDpiX()
        {
            return dpiX;
        }
        static public double GetDpiY()
        {
            return dpiY;
        }

        static public dynamic JSONLoader(string path)
        {
            dynamic json = null;
            string str;
            using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
            {
                str = reader.ReadToEnd();
            }

            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                json = serializer.DeserializeObject(str);
            }
            catch (Exception e)
            {
                LogWriter.write(e.ToString());
                return null;
            }

            return json;
        }


    }
}
