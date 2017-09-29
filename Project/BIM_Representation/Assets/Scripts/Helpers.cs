using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class Helpers
{
    public static string ArrayToString<T>(T[] array, char delim = ',')
    {
        string toRet = "";
        for (int i = 0; i < array.Length; ++i)
        {
            toRet += array[i];
            if (i < array.Length - 1)
                toRet += delim;
        }
        return toRet;
    }

    public static string ListToString<T>(List<T> list, char delim = ',')
    {
        string toRet = "";
        for (int i = 0; i < list.Count; ++i)
        {
            toRet += list[i];
            if (i < list.Count - 1)
                toRet += delim;
        }
        return toRet;
    }

    public static IFCEntityTypes GetEntityType(string s)
    {
        var items = Enum.GetValues(typeof(IFCEntityTypes)).Cast<IFCEntityTypes>().ToList();
        foreach (var item in items)
        {
            if (s.ToUpper().Equals(item.ToString().ToUpper()))
            {
                return item;
            }
        }
        return IFCEntityTypes.NULL;
    }
}