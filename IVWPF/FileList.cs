using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace IVWIN
{


    public class FileSort
    {

        static public void Sort(ref List<VirtualFileInfo> vs, FileSortOption sortOption) {
            switch (sortOption)
            {
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
