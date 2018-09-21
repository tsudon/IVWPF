using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IVWIN
{
    public enum DrawMode
    {
        DEFALT = 0,
        ORIGINAL = 1,
        WIDTH_MATCH = 2,
        HEIGHT_MATCH = 3,
        FRAME_MATCH =4
    }

    public enum FileSortOption
    {
        SORT_DEAULT = 6,
        SORT_BY_NAME = 0,
        SORT_BY_NAME_DESC = 1,
        SORT_BY_DATE = 2,
        SORT_BY_DATE_DESC = 3,
        SORT_BY_SIZE = 4,
        SORT_BY_SIZE_DESC = 5,
        SORT_BY_NAME_LOGICAL = 6,
        SORT_BY_NAME_LOGICAL_DESC = 7,
        SORT_DESC = 1
    }

    public class LoadOption
    {
        public bool isMangaMode = true;
        public DrawMode drawMode = DrawMode.DEFALT;
        public bool isAnimate = true;
        public FileSortOption sortOption = FileSortOption.SORT_BY_NAME_LOGICAL;
        public bool isDirectoryMove = true;
        public bool isArchveRead = false;
        public bool loadPixivAnimation = true;
        public string CurrentFolder;
        public string CurrentFile;
        private string ApplicationName;
        private string Author;
        private string Version;
        private string SavePath;



        public LoadOption(){
            CurrentFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            AssemblyCompanyAttribute acAttribute =
   (System.Reflection.AssemblyCompanyAttribute)
        Attribute.GetCustomAttribute(
    System.Reflection.Assembly.GetExecutingAssembly(),
    typeof(System.Reflection.AssemblyCompanyAttribute));
            System.Reflection.AssemblyTitleAttribute atAttibute =
                (System.Reflection.AssemblyTitleAttribute)
                    Attribute.GetCustomAttribute(
                System.Reflection.Assembly.GetExecutingAssembly(),
                typeof(System.Reflection.AssemblyTitleAttribute));
            ApplicationName = atAttibute.Title;
            Author = acAttribute.Company;
            Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            SavePath = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            SavePath = Path.Combine(SavePath, Author, ApplicationName, "loadoption.info");
            Load();
        }

        public string GetApplicationName()
        {
            return ApplicationName;
        }

        public string GetVersion()
        {
            return Version;
        }



        public void Save()
        {
            LogWriter.write(SavePath);

            try
            {
                string apppath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                apppath = Path.Combine(apppath, Author);
                if ( !Directory.Exists(apppath))
                {
                    Directory.CreateDirectory(apppath);
                }
                apppath = Path.Combine(apppath, ApplicationName);
                if (!Directory.Exists(apppath))
                {
                    Directory.CreateDirectory(apppath);
                }

                using (StreamWriter stream = File.CreateText(SavePath))
                {
                    Type type = typeof(LoadOption);
                    foreach (FieldInfo info in type.GetFields())
                    {
                        stream.WriteLine($"{info.Name}" + " = " + info.GetValue(this));
                    }
                }
            }
            catch
            {

            }

        }

        public void Load()
        {
            LogWriter.write(SavePath);
            try
            {
                Type type = typeof(LoadOption);
                using (StreamReader stream = File.OpenText(SavePath))
                {
                    string line = "";
                    while ((line = stream.ReadLine()) != null)
                    {
                        try
                        {
                            Regex regex = new Regex("^([a-zA-Z0-9_]+)\\s*\\=\\s*(.*)$");
                            string[] key = regex.Split(line);
                            key[1].TrimEnd(' ');
                            FieldInfo info = type.GetField(key[1]);
                            Type t = type.GetField(key[1]).FieldType;
                            LogWriter.write($"#{key[1]}#{key[2]}#{t}");


                            if (t == typeof(bool))
                            {
                                if (key[2] == "True") info.SetValue(this, true);
                                else if (key[2] == "False") info.SetValue(this, true);
                            }
                            else if (t == typeof(string))
                            {
                                info.SetValue(this, key[2]);
                            }
                            else if (t == typeof(int))
                            {
                                info.SetValue(this, int.Parse(key[2]));
                            }
                            else if (t == typeof(double))
                            {
                                info.SetValue(this, double.Parse(key[2]));
                            }
                            else
                            {
                                info.SetValue(this, Enum.Parse(t, key[2]));
                            }
                        }
                        catch(Exception e2)
                        {
                            LogWriter.write(e2.ToString());
                        }
                    }
                }
            }
            catch(Exception e)
            {
                LogWriter.write(e.ToString());
            }

        }

    }
}