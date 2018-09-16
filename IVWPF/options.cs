using System;
using System.IO;
using System.Reflection;

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
        public string ApplicationName {get; private set;}
        public string Author { get; private set; }
        public string Version { get; private set; }


        public LoadOption(){
            CurrentFolder = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            System.Reflection.AssemblyCompanyAttribute acAttribute =
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
        }

        public void Save()
        {
            Type type = typeof(LoadOption);
            foreach(FieldInfo info in type.GetFields())
            {
                    LogWriter.write(info.Name + " = " + info.GetValue(this));
            }

        }
    }
}