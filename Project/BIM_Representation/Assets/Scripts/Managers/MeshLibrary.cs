using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MeshLibrary : Singleton<MeshLibrary>
{
    private static MultiValueDictionary<uint, GameObject> _objectsByTag = new MultiValueDictionary<uint, GameObject>();

    public static void AddGameObject(uint tag, GameObject go)
    {
        _objectsByTag.Add(tag, go);
    }

    public static GameObject[] GetGameObjects(uint tag)
    {
        return _objectsByTag[tag].ToArray();
    }
}
