using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ParseHelpers
{
    public static float StringToFloat(string s)
    {
        float f;
        if (!float.TryParse(s, out f))
        {
            Debug.LogError(s + " can't be parsed to float");
        }
        return f;
    }

    public static float[] StringToFloatArr(string s, char delim = ',')
    {
        var values = s.Split(delim);
        float[] fArr = new float[values.Length];

        for (int i = 0; i < fArr.Length; ++i)
        {
            fArr[i] = StringToFloat(values[i]);
        }
        return fArr;
    }

    public static uint GetId(string line)
    {
        var begin = 1;
        var length = line.IndexOf('=') - begin;
        return uint.Parse(line.Substring(begin, length));
    }

    public static string[] GetValues(string line, string toFind, char delim = ',')
    {
        var begin = line.IndexOf(toFind) + toFind.Length;
        var length = line.Length - toFind.Length - 1 - begin;
        return line.Substring(begin, length).Split(delim);
    }

    public static uint GetValueId(string value)
    {
        return uint.Parse(value.Substring(1));
    }

    public static List<uint> GetValueIds(string line, string toFind, char delim = ',')
    {
        List<uint> ids = new List<uint>();
        var values = GetValues(line, toFind, delim);
        foreach (var value in values)
        {
            ids.Add(GetValueId(value));
        }
        return ids;
    }
}