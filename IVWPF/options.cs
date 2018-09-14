using System;
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
    }
}