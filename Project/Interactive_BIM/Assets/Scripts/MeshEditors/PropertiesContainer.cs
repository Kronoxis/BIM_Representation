using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PropertiesContainer : MonoBehaviour
{
    public List<string> Keys = new List<string>();
    public List<string> Values = new List<string>();

    private Dictionary<string, string> _properties = new Dictionary<string, string>();

    public void CreateProperties(string[] keys, string[] values)
    {
        Keys = keys.ToList();
        Values = values.ToList();
        for (int i = 0; i < keys.Length; ++i)
        {
            _properties.Add(keys[i], values[i]);
        }
    }

    public string GetProperty(string key)
    {
        return _properties[key];
    }

    public string this[string key]
    {
        get { return _properties[key]; }
        set { _properties[key] = value; }
    }
}
