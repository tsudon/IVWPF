using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IVWIN
{

    public enum FileType
    {
        List = 0,
        Directory = 1,
        File = 2,
        FileOrDirectory = 3,
        Archive = 4,    // .zip
        InfoText = 8,    // .text .txt .json
        Unknown = -1
    }


    public class VirtualFileInfo
    {
        public FileType Type { get; private set; }
        public string FullName { get; private set; }
        public string Name { get; private set; }
        public long Length { get; private set; }
        public DateTime CreationTime { get; private set; }
        public DateTime CreationTimeUtc { get; private set; }
        public FileAttributes Attributes { get; private set; }
        public string DirectoryFullName { get; private set; }
        public string DirectoryName { get; private set; }
        public string Parent { get; private set; }


        public VirtualFileInfo(String path,int option)
        {
            Uri uri = new Uri(path);
            if (uri.Scheme.Equals("file"))
            {
                if (File.Exists(path))
                {
                    FileInfo info = new FileInfo(path);
                    Name = info.Name;
                    FullName = info.FullName;
                    DirectoryFullName = info.Directory.FullName;
                    DirectoryName = info.Directory.Name;
                    Length = info.Length;
                    CreationTime = info.CreationTime;
                    CreationTimeUtc = info.CreationTimeUtc;
                    Attributes = info.Attributes;
                    DirectoryInfo directoryInfo = new DirectoryInfo(DirectoryFullName);
                    Parent = directoryInfo.Parent.FullName;

                    Type = FileType.File;

                    String ext = info.Extension.ToLower(); 
                    if (ext == ".txt" || ext == ".json" || ext == ".text")
                    {
                        Type = FileType.InfoText;
                    } else if ( ext == ".zip")
                    {
                        Type = FileType.Archive;
                    }
                }
                else if (System.IO.Directory.Exists(path))
                {
                    DirectoryInfo info = new DirectoryInfo(path);
                    Name = info.Name;
                    FullName = info.FullName;
                    DirectoryFullName = info.FullName;
                    DirectoryName = info.Name;
                    Length = -1;
                    CreationTime = info.CreationTime;
                    CreationTimeUtc = info.CreationTimeUtc;
                    Attributes = info.Attributes;
                    DirectoryInfo directoryInfo = new DirectoryInfo(DirectoryFullName);
                    Parent = directoryInfo.Parent.FullName;
                    Type = FileType.Directory;
                }
                else
                {
                    Type = FileType.Unknown;
                    return;
                }

            }

         }

    }




    public class FileManager
    {
        private string path;
        private string currentFileName;
        private string currentDir;
        private List<VirtualFileInfo> directryListInfo = new List<VirtualFileInfo>();
        private VirtualFileInfo[] infos;
        List<VirtualFileInfo> dirList;
        private VirtualFileInfo[] dirInfos;
        private int currentDirPos;
        private int parentDirPos =0,parentLength =1;
        private LoadOption loadOption;

        private int nowpos;


        public FileManager(string path,LoadOption loadOption)
        {
            this.path = path;
            this.loadOption = loadOption;
            GetImagePath(path);
        }


        public string GetImagePath(string path)
        {
            SearchDirectry(path);
            string imagePath = currentDir + "\\" + currentFileName;
            LogWritter.write("Get path:" + imagePath);

            return imagePath;
        }


        private void ListDirectory()
        {
            directryListInfo.Clear();
            foreach (string f in Directory.GetFileSystemEntries(currentDir))
            {
                directryListInfo.Add(new VirtualFileInfo(f, 0));
            }
            FileSort.Sort(ref directryListInfo, loadOption.sortOption);
            infos = directryListInfo.ToArray();
        }

        public void SearchDirectry(String path)
        {
            SearchDirectry(path, false, 0);

        }


        public string GetParent(string path,int step)
        {
            String dir = path;

            if(dirList != null)
            {
                parentDirPos += step;
                if (parentDirPos > 0 && parentDirPos < dirInfos.Length)
                {
                    return dirList[parentDirPos].DirectoryFullName;
                }
                return null;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(dir);
            if (directoryInfo.FullName == null) return null;

            if (System.IO.Directory.Exists(path))
            {
                currentFileName = path.Substring(path.TrimEnd('\\').LastIndexOf('\\')).Trim('\\');
                currentDir = path.Substring(0,path.TrimEnd('\\').LastIndexOf('\\')).TrimEnd('\\');

            }
            else
            {
                currentDir = directoryInfo.Parent.FullName;
                currentFileName = directoryInfo.Name;
            }


            ListDirectory();

            dirList = new List<VirtualFileInfo>();
            foreach (VirtualFileInfo info in infos)
            {
                if (info.Type == FileType.Directory)
                {
                    dirList.Add(info);

                }
            }
            dirInfos = dirList.ToArray();
            int j = 0, pos = 0;
            foreach (VirtualFileInfo info in dirInfos)
            {
                if (info.Name.Equals(currentFileName))
                {
                    pos = j;
                }
                j++;
            }

            pos += step;

            if (pos > dirInfos.Length - 1) pos = 0;
            if (pos < 0) pos = dirInfos.Length - 1;

            parentLength = dirInfos.Length;
            parentDirPos = pos;

            return dirInfos[pos].DirectoryFullName;
        }

        public void SearchDirectry(string path,bool option, int step)
        {

            if (option)
            {
                string p = GetParent(path, step);
                if (p == null) return;
                SearchDirectry(p, false, 0);
                return;
            }
            else
            if (File.Exists(path))
            {
                LogWritter.write("Drop File is exist.");

                currentFileName = Path.GetFileName(path);
                currentDir = Path.GetDirectoryName(path);

            }
            else if (Directory.Exists(path))
            {
                LogWritter.write("Drop file is directry");
                currentDir = path;
                currentFileName = null;
            }
            else
            {
                return;
            }

            ListDirectory();

            int i = 0;
            foreach (VirtualFileInfo info in infos)
            {
                if (info.Name.Equals(currentFileName))
                {
                     currentDirPos = i;
                     break;
                }
                i++;
            }

            if (currentFileName == null)
            {
                currentDirPos = 0;
                if (infos.Length > 0)
                {
                    currentFileName = infos[0].Name;
                }
                else
                {
                    LogWritter.write("This Directry is empty.");
                    currentFileName = null;
                }
            }
            nowpos = currentDirPos;
            LogWritter.write("Set current path is " + currentDir);
            LogWritter.write("Set current file is " + currentFileName);
        }


        public void MarkDirectoryOffset()
        {
            this.nowpos = currentDirPos;
        }

        private bool isLooped = false;

        public string GetNextPath()
        {
            int i = currentDirPos;
            isLooped = false;
            do
            {
                i++;
                if (i == nowpos) { isLooped = true; return null; }
                if (i >= infos.Length)
                {
                    i = 0;
                    if (loadOption.isDirectoryMove)
                    {
                        string path = currentDir + "\\" + currentFileName;
                        if (Directory.Exists(path) ) {
                            path = currentDir;
                        }
                        SearchDirectry(infos[currentDirPos].DirectoryFullName, true, +1);
                        currentDirPos = 0;
                        i = currentDirPos;
                        nowpos = currentDirPos;
                    }
                    else
                    {
                        currentDirPos = 0;
                    }
                }
            } while (infos[i].Type != FileType.File);
            currentDirPos = i;
            currentFileName = infos[currentDirPos].Name;
            string imagePath = currentDir + "\\" + currentFileName;
            return imagePath;
        }

        public string GetPreviousPath()
        {
            int i = currentDirPos;
            do
            {
                i--;
                if (i == nowpos) { isLooped = true; return null; }
                if (i < 0)
                {
                    i = infos.Length -1;
                    if (loadOption.isDirectoryMove)
                    {
                        string path = currentDir + "\\" + currentFileName;
                        if (Directory.Exists(path))
                        {
                            path = currentDir;
                        }
                        SearchDirectry(infos[currentDirPos].DirectoryFullName,true, -1);

                        if (parentDirPos < 0) return null;
                        currentDirPos = infos.Length -1;
                        i = currentDirPos;
                        nowpos = currentDirPos;
                    }
                    else
                    {
                        currentDirPos = 0;
                    }
                }
            } while (infos[i].Type != FileType.File);
            currentDirPos = i;
            currentFileName = infos[currentDirPos].Name;
            string imagePath = currentDir + "\\" + currentFileName;
            return imagePath;
        }

        public bool GetLooped()
        {

            return isLooped;
        }
    }
}
