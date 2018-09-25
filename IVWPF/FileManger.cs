using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
            try
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



                        String ext = info.Extension.ToLower();
                        Regex regex = new Regex("\\.(bmp|dib|rle|ico|icon|gif|jpeg|jpe|jpg|jfif|exif|png|tiff|tif)$");
                        if (regex.IsMatch(ext) )
                        {
                            Type = FileType.File;

                        }
                        else if (ext == ".txt" || ext == ".json" || ext == ".text" || ext == ".text")
                        {
                            Type = FileType.InfoText;
                        }
                        else if (ext == ".zip")
                        {
                            Type = FileType.Archive;
                        }
                        else 
                        {
                            Type = FileType.Unknown;
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
            catch
            {

            }
         }


    }


    /* 書き直した方が早いぐらいな酷いコード　*/



    class VirtualFileList
    {
        public string currentFileName;
        public string currentDir;
        public string currentFullName;
        private List<VirtualFileInfo> directryListInfo = new List<VirtualFileInfo>();
        public VirtualFileInfo[] infos;
        private List<VirtualFileInfo> dirList = new List<VirtualFileInfo>();
        public VirtualFileInfo[] dirInfos;
        public VirtualFileList Parent { get; private set; }
        public int currentPos;
        public int dirPos = 0, parentLength = 1;
        public int nowpos;
        private LoadOption loadOption;



        public VirtualFileList(LoadOption loadOption)
        {
            this.loadOption = loadOption;
        }

        //ディレクトリ情報をリスト化
        public void ListDirectory(String directroy)
        {
            directryListInfo.Clear();
            dirList.Clear();
            List<VirtualFileInfo> list = new List<VirtualFileInfo>();
            foreach (string f in Directory.GetFileSystemEntries(directroy))
            {

                VirtualFileInfo info = new VirtualFileInfo(f, 0);
                directryListInfo.Add(info);
                if (info.Type == FileType.File)
                {
                    list.Add(info);
                }
                else if (info.Type == FileType.Directory)
                {
                    dirList.Add(info);
                }
                else if (loadOption.loadPixivAnimation && info.Type == FileType.Archive)
                {
                    string path = info.FullName;
                    string jsonPath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path)) + ".json";
                    if (File.Exists(jsonPath))
                    {
                        list.Add(info);
                    }
                }
            }
            FileSort.Sort(ref directryListInfo, loadOption.sortOption);
            FileSort.Sort(ref list, loadOption.sortOption);
            FileSort.Sort(ref dirList, loadOption.sortOption);
            infos = list.ToArray();          
            dirInfos = dirList.ToArray();
        }

        //親ディレクトリを取得 =0 current -1 previous +1 next
        public string GetParent(string path, int step)
        {
            String dir = path;

            if (Parent != null)
            {
                Parent.dirPos += step;
                if (Parent.dirPos < 0)
                {
                    Parent.dirPos = Parent.dirInfos.Length - 1;
                }

                if (Parent.dirPos > Parent.dirInfos.Length - 1)
                {
                    Parent.dirPos = 0;
                }

            }
            else
            {

                Parent = new VirtualFileList(loadOption);
                Parent.SearchDirectry(infos[currentPos].Parent);

                Parent.SearchCurrentDirectryPos(infos[currentPos].DirectoryFullName);

                DirectoryInfo directoryInfo = new DirectoryInfo(dir);
                if (directoryInfo.FullName == null) return null;
                Parent.dirPos += step;
                if (Parent.dirPos < 0)
                {
                    Parent.dirPos = Parent.dirInfos.Length - 1;
                }

                if (Parent.dirPos > Parent.dirInfos.Length - 1)
                {
                    Parent.dirPos = 0;
                }

            }
            return Parent.GetDirectory();
        }

        //ディレクトリ名からインデックスを取得
        //ハッシュ管理してない。ファイルが1000も無いかぎりパフォーマンスに差は無い（たぶん）
        public int SearchCurrentDirectryPos(string dirpath)
        {
            LogWriter.write(dirpath);
            if (Directory.Exists(dirpath))
            {
                for (int i = 0; i < dirInfos.Length; i++)
                {
                    LogWriter.write($"{i}:{dirInfos[i].FullName}");
                    if (dirInfos[i].FullName == dirpath)
                    {
                        dirPos = i;
                        break;
                    }
                }
            }
            else
            {
                dirPos = 0;
                return -1;
            }
            LogWriter.write($"{dirPos}");

            return dirPos;
        }

        public string GetDirectory()
        {
            LogWriter.write($"GetDirectroy {dirPos} Position");

            return dirInfos[dirPos].DirectoryFullName;
        }



        //option false => 同じフォルダ
        //option true => フォルダをstep移動

        public string SearchDirectry(string path, bool option, int step)
        {
            if (option)
            {
                LogWriter.write($"{path}:{step}");
                String str = GetParent(path, step);
                if (str == null)
                {
                    return null;
                }
                LogWriter.write("path:" +str);
                return SearchDirectry(str, false, 0);
            }
            if (File.Exists(path))
            {
                LogWriter.write("Drop File is exist.");

                currentFileName = Path.GetFileName(path);
                currentDir = Path.GetDirectoryName(path);
                currentFullName = Path.Combine(currentFileName,currentDir);

            }
            else if (Directory.Exists(path))
            {
                LogWriter.write("Drop file is directry");
                currentDir = path;
                currentFileName = null;
                currentFullName = path;
            }
            else
            {
                return null;
            }

            ListDirectory(currentDir);

            int i = 0;

            foreach (VirtualFileInfo info in infos)
            {
                if (info.Name.Equals(currentFileName))
                {
                    currentPos = i;
                    break;
                }
                i++;
            }


            if (currentFileName == null)
            {
                currentPos = 0;
                string str = SetCurrentPos(0);
                if (str == null)
                {
                    LogWriter.write("This Directry is empty.");
                    currentFileName = null;
                    return null;
                }
            }

            nowpos = currentPos;
            string imagePath =Path.Combine( currentDir , currentFileName);
            return imagePath;
        }

        public string GetNextPath(bool flag)
        {
            int i = currentPos;
            int old = currentPos;
            isLooped = false;


            do
            {
                i++;
                if (i >= infos.Length)
                {
                    if (!flag) { i = infos.Length - 1; return null; }
                    i = 0;
                    if (loadOption.isDirectoryMove && flag)
                    {
                        string name = (currentFileName != null) ? currentFileName : "";
                        string path = Path.Combine(currentDir , name);
                        if (Directory.Exists(path))
                        {
                            path = currentDir;
                        }
                        do
                        {
                            currentFullName = SearchDirectry(currentDir, true, +1);
                        } while (currentFullName == null);
                        currentPos = 0;
                        i = currentPos;
                        nowpos = currentPos;
                    }
                    else
                    {
                        currentPos = 0;
                    }
                }
            } while (infos[i].Type != FileType.File && infos[i].Type != FileType.Archive);
            if (flag)
            {
                currentPos = i;
                currentFileName = infos[currentPos].Name;
            }
            else
            {
                currentPos = old;
            }
            string imagePath = Path.Combine(currentDir, infos[i].Name);
            return imagePath;
        }

        public string GetPreviousPath(bool flag)
        {
            int i = currentPos;
            int old = currentPos;
            do
            {
                i--;
                //              if (i == nowpos) { isLooped = true; return null; }
                if (i < 0)
                {
                    if (!flag) { i = 0; return null; }
                    i = infos.Length - 1;
                    if (loadOption.isDirectoryMove && flag)
                    {
                        string name = (currentFileName != null) ? currentFileName : "";
                        string path = Path.Combine(currentDir, name);
                        if (Directory.Exists(path))
                        {
                            path = currentDir;
                        }
                        do
                        {
                            currentFullName = SearchDirectry(currentDir, true, -1);
                        } while (currentFullName == null);
                        if (Parent.dirPos < 0) return null;
                        currentPos = infos.Length - 1;
                        i = currentPos;
                        nowpos = currentPos;
                    }
                    else
                    {
                        currentPos = infos.Length - 1;
                    }
                }
            } while (infos[i].Type != FileType.File && infos[i].Type != FileType.Archive);

            if (flag)
            {
                currentPos = i;
                currentFileName = infos[currentPos].Name;
            }
            else
            {
                currentPos = old;
            }
            string imagePath = Path.Combine(currentDir, infos[i].Name);

            return imagePath;
        }

        public string GetImagePath(string path)
        {
            if (path == null) return null; 
            SearchDirectry(path);
            string imagePath = Path.Combine(currentDir,currentFileName);
            return imagePath;
        }

        private bool isLooped = false;


        public string SearchDirectry(int step)
        {
            string imagePath = currentFileName == null ? currentDir :Path.Combine(currentDir, currentFullName);
            return SearchDirectry(imagePath,true, step);

        }


        public string SearchDirectry(String path)
        {
            return SearchDirectry(path, false, 0);

        }

        public bool GetLooped()
        {

            return isLooped;
        }

        public string GetCurrentFullName()
        {
            currentFileName = infos[currentPos].Name;
            currentDir = infos[currentPos].DirectoryFullName;
            currentFullName = Path.Combine(currentDir, currentFileName);
            return currentFullName;
        }

        internal string SetCurrentPos(int pos)
        {
            if (infos.Length < 1) return null;
            if (pos > infos.Length) pos = 0;
            if (pos < 0) pos = infos.Length - 1;
            currentPos = pos;
            currentFileName = infos[pos].Name;
            while (infos[pos].Type != FileType.File)
            {
                pos++;
                if (pos >= infos[pos].Length) return null;
                currentPos = pos;
            }
            return GetCurrentFullName();
        }
    }


    public class FileManager
    {
        private string path;
        private LoadOption loadOption;
        private VirtualFileList list;
       
        public string imagePath;

        public FileManager(string path,LoadOption loadOption)
        {
            this.path = path;
            this.loadOption = loadOption;
            list = new VirtualFileList(loadOption);
            imagePath = list.GetImagePath(path);
        }

        internal string GetImagePath(string imagePath)
        {
            imagePath = list.GetImagePath(imagePath);
            return imagePath;
        }

        //false = ファイル位置（ポインタ）を移動しないで、次／前のファイルを返す
        //true = ファイル位置（ポインタ）を移動して、次／前のファイルを返す

        internal string GetPreviousPath(bool flag)
        {
            imagePath = list.GetPreviousPath(flag);
            return imagePath;
        }

        internal string GetNextPath(bool flag)
        {
            imagePath = list.GetNextPath(flag);
            return imagePath;
        }

        internal string GetPreviousPath()
        {
            imagePath = list.GetPreviousPath(true);
            return imagePath;
        }

        internal string GetNextPath()
        {
            imagePath = list.GetNextPath(true);
            return imagePath;
        }

        internal void SetCurrentPath(string path)
        {
            list.SearchDirectry(path);
            imagePath = path;
        }

        internal string GetPathFormPos(int pos)
        {
            if (pos < 0) pos = list.infos.Length -1;
            if (pos > list.infos.Length) pos = 0;
            imagePath = list.SetCurrentPos(pos);
            return imagePath;
        }

        internal string GetNextFolderFile()
        {
            list.SearchDirectry(+1);
            imagePath = list.SetCurrentPos(0);
            return imagePath;
        }

        internal string GetPreviousFolderFile()
        {
            list.SearchDirectry(-1);
            imagePath = list.SetCurrentPos(-1);
            return imagePath;
        }

        internal string GetFolder()
        {
            return list.currentDir;
        }

        internal int GetParentCount()
        {
            if (list.Parent == null) list.SearchDirectry(0);
            return list.Parent.dirInfos.Length;
        }

        internal string GetCurrentPath()
        {
            return list.GetCurrentFullName();
        }
    }
}
