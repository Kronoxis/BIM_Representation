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
        var entityTypes = Enum.GetValues(typeof(IFCEntityTypes)).Cast<IFCEntityTypes>().ToList();
        foreach (var type in entityTypes)
        {
            if (s.ToUpper().Equals(type.ToString().ToUpper()))
            {
                return type;
            }
        }
        return IFCEntityTypes.NULL;
    }

    #region Delegate Conversions

    public static uint FuncToUint(string s)
    {
        return uint.Parse(s.Substring(1));
    }
    public delegate uint DelToUint(string s);
    public static Func<string, uint> ConvertToUint = FuncToUint;

    public static int FuncToInt(string s)
    {
        return int.Parse(s);
    }
    public delegate int DelToInt(string s);
    public static Func<string, int> ConvertToInt = FuncToInt;

    public static float FuncToFloat(string s)
    {
        return float.Parse(s);
    }
    public delegate float DelToFloat(string s);
    public static Func<string, float> ConvertToFloat = FuncToFloat;

    public static bool FuncToBool(string s)
    {
        return s == ".T.";
    }
    public delegate bool DelToBool(string s);
    public static Func<string, bool> ConvertToBool = FuncToBool;

    public static string FuncToString(string s)
    {
        return s;
    }
    public delegate string DelToString(string s);
    public static Func<string, string> ConvertToString = FuncToString;
    #endregion
}