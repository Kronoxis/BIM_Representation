using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Helpers
{
    public static string ToString<T>(this T[] array, char delim)
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

    public static string ToString<T>(this List<T> list, char delim)
    {
        return list.ToArray().ToString(delim);
    }

    public static uint AsId(this string s)
    {
        if (s == "") return 0;
        return uint.Parse(s.Substring(1));
    }

    public static int AsInt(this string s)
    {
        if (s == "") return 0;
        return int.Parse(s);
    }

    public static float AsFloat(this string s)
    {
        if (s == "") return 0;
        return float.Parse(s);
    }

    public static bool AsBool(this string s)
    {
        if (s == "") return false;
        return s == ".T.";
    }

    public static string AsString(this string s)
    {
        return s;
    }

    public static T AsEnum<T>(this string s) where T : IConvertible
    {
        if (s == "") return default(T);
        var matches = Enum.GetValues(typeof(T)).Cast<T>()
            .Where(e => s.Substring(1, s.Length - 2).ToUpper().Equals(e.ToString().ToUpper())).ToArray();
        if (matches.Length < 1) return default(T);
        return matches[0];
    }

    public static List<string> AsList(this string s, char delim = ',')
    {
        return SplitProperties(s, delim);
    }

    public static List<uint> AsIds(this List<string> l)
    {
        List<uint> toRet = new List<uint>();
        foreach (var s in l)
        {
            toRet.Add(s.AsId());
        }
        return toRet;
    }

    public static List<IFCEntity> ToEntityList(this List<uint> ids, IFCDataContainer container)
    {
        return container.GetEntities(ids);
    }

    public static List<string> SplitProperties(string s, char delim)
    {
        List<string> properties = new List<string>();

        StringBuilder sb = new StringBuilder();
        bool isList = false;
        bool isString = false;
        char stringChar = '\'';
        var buffer = s.ToCharArray();
        foreach (var c in buffer)
        {
            // ' or " mark begin/end of string
            if ((isString && c == stringChar) ||
                (!isString && (c == '"' || c == '\'')))
            {
                stringChar = c;
                isString = !isString;
                continue;
            }

            // Not a string
            if (!isString)
            {
                // Open list
                if (c == '(')
                {
                    isList = true;
                    sb.Clear();
                    continue;
                }

                // Close list
                if (c == ')')
                {
                    isList = false;
                    continue;
                }

                // Skip $ and *
                if (c == '$' || c == '*')
                {
                    continue;
                }

                // Value seperator while not in list
                if (!isList && c == delim)
                {
                    properties.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }
            }

            // Add character
            sb.Append(c);
        }
        properties.Add(sb.ToString());
        return properties;
    }

    public static void Add<TKey, TValue>(this Dictionary<TKey, TValue> d, KeyValuePair<TKey, TValue> key)
    {
        d.Add(key.Key, key.Value);
    }

    public static void Clear(this StringBuilder sb)
    {
        sb.Remove(0, sb.Length);
    }

    private static string[] _byteUnits = {"", "k", "M", "G", "T"};
    public static string BytesToString(long bytes)
    {
        float remaining = bytes;
        int unit = 0;
        while (remaining > 1024) {++unit; remaining /= 1024;}
        string unitStr = unit == 0 ? "bytes" : _byteUnits[unit] + "B";
        return remaining.ToString("0.0") + " " + unitStr;
    }
}