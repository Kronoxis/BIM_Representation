using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Helpers
{
    public static string ArrayToString<T>(T[] array, char delim = ',')
    {
        StringBuilder toRet = new StringBuilder();
        for (int i = 0; i < array.Length; ++i)
        {
            toRet.Append(array[i]);
            if (i < array.Length - 1)
                toRet.Append(delim);
        }
        return toRet.ToString();
    }

    public static string ListToString<T>(List<T> list, char delim = ',')
    {
        return ArrayToString(list.ToArray(), delim);
    }

    #region Delegate Conversions

    public static uint PropertyToId(string s)
    {
        if (s == "$" || s == "*") return 0;
        return uint.Parse(s.Substring(1));
    }

    public static int PropertyToInt(string s)
    {
        if (s == "$" || s == "*") return 0;
        return int.Parse(s);
    }

    public static float PropertyToFloat(string s)
    {
        if (s == "$" || s == "*") return 0;
        return float.Parse(s);
    }

    public static bool PropertyToBool(string s)
    {
        if (s == "$" || s == "*") return false;
        return s == ".T.";
    }

    public static string PropertyToString(string s)
    {
        if (s == "$" || s == "*") return "";
        if (s.IndexOf('\'') == 0) return s.Substring(1, s.Length - 2);
        return s;
    }

    public static T PropertyToEnum<T>(string s) where T : IConvertible
    {
        if (s == "$") return default(T);
        var matches = Enum.GetValues(typeof(T)).Cast<T>()
            .Where(e => s.Substring(1, s.Length - 2).ToUpper().Equals(e.ToString().ToUpper())).ToArray();
        if (matches.Length < 1) return default(T);
        return matches[0];
    }

    #endregion

    public static uint ToIdProperty(this string s)
    {
        if (s == "$" || s == "*") return 0;
        return uint.Parse(s.Substring(1));
    }

    public static int ToIntProperty(this string s)
    {
        if (s == "$" || s == "*") return 0;
        return int.Parse(s);
    }

    public static float ToFloatProperty(this string s)
    {
        if (s == "$" || s == "*") return 0;
        return float.Parse(s);
    }

    public static bool ToBoolProperty(this string s)
    {
        if (s == "$" || s == "*") return false;
        return s == ".T.";
    }

    public static string ToStringProperty(this string s)
    {
        if (s == "$" || s == "*") return "";
        if (s.IndexOf('\'') == 0) return s.Substring(1, s.Length - 2);
        return s;
    }

    public static T ToEnumProperty<T>(this string s) where T : IConvertible
    {
        if (s == "$") return default(T);
        var matches = Enum.GetValues(typeof(T)).Cast<T>()
            .Where(e => s.Substring(1, s.Length - 2).ToUpper().Equals(e.ToString().ToUpper())).ToArray();
        if (matches.Length < 1) return default(T);
        return matches[0];
    }
}