using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace IVWIN
{
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
            uint dpix=(uint)dpiX, dpiy=(uint)dpiY;
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
    }
}
