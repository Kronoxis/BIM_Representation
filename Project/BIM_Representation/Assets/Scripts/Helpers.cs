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
        char[] delims = {delim};
        var values = s.Split(delims);
        float[] fArr = new float[values.Length];

        for (int i = 0; i < fArr.Length; ++i)
        {
            fArr[i] = StringToFloat(values[i]);
        }
        return fArr;
    }

    //public static void AddEntity(this Dictionary<uint, IfcEntity> d, string line)
    //{
    //    var id = uint.Parse(line.Substring(1, line.IndexOf('=') - 1));
    //    var entity = new IfcEntity(line);
    //    d.Add(id, entity);
    //}
}