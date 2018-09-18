using System;


/// <summary>
/// ファイルポイントの移動方向
/// </summary>
public enum FileManagerMoveOrder
{
    Current =0,
    Next = 1,
    Previous = -1,
    Random = 2
}

/// <summary>
/// フォルダーをまたぐか？オプション
/// </summary>
public enum FileManagerMoveFolder
{
    Loop = 0,
    Next = 1,
    Previous = -1,
}


/// <summary>
/// FileManagerの実装し直し版 分割することで、互換を保ちつつ移行する(予定)
/// </summary>
public partial class FileManager
{
    private FileManagerMoveOrder MoveFlag;

    internal SetOrder(FileManagerMoveFlag flag)
    {
        MoveFlag = flag;
    }

    internal string GetPath() {
        list.SetIndex((int)MoveFlag);
    }
}


