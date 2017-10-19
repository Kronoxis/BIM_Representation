using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshLibrary : Singleton<MeshLibrary>
{
    private static MultiValueDictionary<uint, GameObject> _objectsByTag = new MultiValueDictionary<uint, GameObject>();
    private static MultiValueDictionary<string, GameObject> _objectsByType = new MultiValueDictionary<string, GameObject>();

    public static void AddGameObject(uint tag, string type, GameObject go)
    {
        _objectsByTag.Add(tag, go);
        _objectsByType.Add(type, go);
    }

    public static GameObject[] GetGameObjects(uint tag)
    {
        return _objectsByTag[tag].ToArray();
    }

    public static GameObject[] GetGameObjects(string type)
    {
        return _objectsByType[type].ToArray();
    }
}
