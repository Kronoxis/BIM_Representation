using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class IFCDataManager
{
    private static Dictionary<FileInfo, int> _containerIds = new Dictionary<FileInfo, int>();
    private static List<IFCDataContainer> _data = new List<IFCDataContainer>();

    #region Add
    public static void AddDataContainer(FileInfo file, IFCDataContainer container)
    {
        _containerIds[file] = _data.Count;
        _data.Add(container);
        container.Index = _containerIds[file];
    }
    #endregion

    #region Set
    public static void SetDataContainer(int index, IFCDataContainer container)
    {
        _data[index] = container;
    }

    public static void SetDataContainer(FileInfo file, IFCDataContainer container)
    {
        _data[_containerIds[file]] = container;
    }
    #endregion

    #region Remove
    public static void RemoveDataContainer(int index)
    {
        _data.RemoveAt(index);
        _containerIds.Remove(GetContainerFile(index));
    }

    public static void RemoveDataContainer(FileInfo file)
    {
        _data.RemoveAt(GetContainerId(file));
        _containerIds.Remove(file);
    }
    #endregion

    #region Get
    public static IFCDataContainer GetDataContainer(int index)
    {
        return _data[index];
    }

    public static IFCDataContainer GetDataContainer(FileInfo file)
    {
        return _data[_containerIds[file]];
    }

    public static int GetContainerId(FileInfo file)
    {
        return _containerIds[file];
    }

    public static FileInfo GetContainerFile(int index)
    {
        var file = _containerIds.Where(c => c.Value == index).ToArray()[0].Key;
        return file;
    }

    public static List<IFCDataContainer> GetAllData()
    {
        return _data;
    }

    public static bool IsParsing()
    {
        int c;
        return IsParsing(out c);
    }

    public static bool IsParsing(out int count)
    {
        count = _data.Where(c => !c.IsParsed()).ToArray().Length;
        return count != 0;
    }

    public static int GetWorkingParserCount()
    {
        int c;
        IsParsing(out c);
        return c;
    }
    #endregion
}
