using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class Helpers
{
    private static IEnumerable<Type> _entityTypes = typeof(IFCEntity).Assembly.GetTypes()
        .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(IFCEntity)));

    private static Dictionary<string, Type> _entityTypesMap = new Dictionary<string, Type>();

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

    public static Type GetEntityType(string s)
    {
        if (!_entityTypesMap.ContainsKey(s))
            _entityTypesMap[s] = FindMatchingType(s);
        return _entityTypesMap[s];
    }

    private static Type FindMatchingType(string s)
    {
        var matches = _entityTypes.Where(t => t.ToString().ToUpper().Equals(s.ToUpper())).ToArray();
        if (matches.Length < 1) return typeof(IFCEntity);
        return matches[0];
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
        return s;
    }

    public static T PropertyToEnum<T>(string s) where T : IConvertible
    {
        if (s == "$") return default(T);
        var matches = Enum.GetValues(typeof(T)).Cast<T>().Where(e => s.Substring(1, s.Length - 2).ToUpper().Equals(e.ToString().ToUpper())).ToArray();
        if (matches.Length < 1) return default(T);
        return matches[0];
    }
    #endregion
}