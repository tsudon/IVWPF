using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace IVWIN
{


    public class FileSort
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        private const string digit = "[0-9]+";
        private const string disitZenkaku = "[０-９]+";
        private const string fdigit = "[0-9]+\\.[0-9]+";
        private const string fdisitZenkaku = "[０-９]+\\.[０-９]+";
        private const string kanjidigit = "[一二三四五六七八九〇十百千万憶兆京]+";
        private const string kanjidigit2 = "[壱壹壹弌弐贰貳貮弎参參叁參叄肆肆䦉伍陆陸漆柒質柒捌玖拾廿卄卅丗卌陌佰阡仟萬憶兆京]+";
        private const string sign = "[\\!\\;\\®µ§»★]";


        public static void Sort(ref List<VirtualFileInfo> vs, FileSortOption sortOption) {

            switch (sortOption)
            {
                case FileSortOption.SORT_BY_NAME_LOGICAL:
                    SortByNameLogical(ref vs);
                    break;
                case FileSortOption.SORT_BY_NAME_LOGICAL_DESC:
                    SortByNameLogicalDesc(ref vs);
                    break;
                case FileSortOption.SORT_BY_NAME:
                    SortByName(ref vs);
                    break;
                case FileSortOption.SORT_BY_NAME_DESC:
                    SortByNameDesc(ref vs);
                    break;
                case FileSortOption.SORT_BY_DATE:
                    SortByDate(ref vs);
                    break;
                case FileSortOption.SORT_BY_DATE_DESC:
                    SortByDateDesc(ref vs);
                    break;
                case FileSortOption.SORT_BY_SIZE:
                    SortBySize(ref vs);
                    break;
                case FileSortOption.SORT_BY_SIZE_DESC:
                    SortBySizeDesc(ref vs);
                    break;

            }
        }

        private static int StrCmpLogicalX(string x,String y)
        {

            return StrCmpLogicalW(x, y);
            /*
                        int r = 0,i=0;
                        while (i < Math.Min(x.Length, y.Length){
                            string xx = x[i].ToString();
                            string yy = x[i].ToString();

                        }

                        if (r == 0) r = x.Length - y.Length;
                        return r;
            return (String.Compare(x, y));
                        */

        }

        private static void SortByNameLogical(ref List<VirtualFileInfo> vs)
        {
            vs.Sort(delegate (VirtualFileInfo x, VirtualFileInfo y)
            {
                return StrCmpLogicalX(x.Name, y.Name);
            });
        }

        private static void SortByNameLogicalDesc(ref List<VirtualFileInfo> vs)
        {
            vs.Sort(delegate (VirtualFileInfo x, VirtualFileInfo y)
            {
                return StrCmpLogicalX(y.Name, x.Name);
            });
        }

        static public void SortByName (ref List<VirtualFileInfo> vs){
            vs.Sort(delegate (VirtualFileInfo x, VirtualFileInfo y)
                {
                    return x.Name.CompareTo(y.Name);
                });
       }

        static public void SortByNameDesc (ref List<VirtualFileInfo> vs)
        {
            vs.Sort(delegate (VirtualFileInfo x, VirtualFileInfo y)
                {
                    return y.Name.CompareTo(x.Name);
                });
       }


       static public void SortByDate(ref List<VirtualFileInfo> vs)
       {
            vs.Sort(delegate (VirtualFileInfo x, VirtualFileInfo y)
            {
                return x.CreationTime.CompareTo(y.CreationTime);
            });
        }

        static public void SortByDateDesc(ref List<VirtualFileInfo> vs)
        {
            vs.Sort(delegate (VirtualFileInfo x, VirtualFileInfo y)
            {
                return y.CreationTime.CompareTo(x.CreationTime);
            });

        }

        static public void SortBySize(ref List<VirtualFileInfo> vs)
        {
            vs.Sort(delegate (VirtualFileInfo x, VirtualFileInfo y)
            {
                if ((x.Attributes & FileAttributes.Directory)== FileAttributes.Directory ) 
                {
                        if ((y.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            return x.Name.CompareTo(y.Name); ;
                        }
                        return -1;
                }

                if ((y.Attributes & FileAttributes.Directory) == FileAttributes.Directory ) {
                   return 1;
                }

                FileInfo xx = new FileInfo(x.FullName);
                FileInfo yy = new FileInfo(y.FullName);
                return xx.Length.CompareTo(yy.Length);
            });
        }

        static public void SortBySizeDesc(ref List<VirtualFileInfo> vs)
        {
            vs.Sort(delegate (VirtualFileInfo y, VirtualFileInfo x)
            {
                if ((x.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    if ((y.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        return x.Name.CompareTo(y.Name); ;
                    }
                    return -1;
                }

                if ((y.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    return 1;
                }

                FileInfo xx = new FileInfo(x.FullName);
                FileInfo yy = new FileInfo(y.FullName);
                return xx.Length.CompareTo(yy.Length);
            });
        }

    }
}
