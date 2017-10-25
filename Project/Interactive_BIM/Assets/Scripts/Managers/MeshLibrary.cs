using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class MeshLibrary : MonoBehaviour
{
    private static MultiValueDictionary<uint, GameObject> _objectsByTag = new MultiValueDictionary<uint, GameObject>();
    private static MultiValueDictionary<string, GameObject> _objectsByType = new MultiValueDictionary<string, GameObject>();

    public static void AddGameObject(uint tag, string type, GameObject go)
    {
        if (!_objectsByTag.ContainsValue(tag, go))
            _objectsByTag.Add(tag, go);
        if (!_objectsByType.ContainsValue(type, go))
            _objectsByType.Add(type, go);
    }

    public static void RemoveGameObject(GameObject go)
    {
        _objectsByTag.Remove(go.GetComponent<MeshTags>().Tag, go);
        _objectsByType.Remove(go.GetComponent<MeshTags>().IfcType, go);
    }

    public static void Clear()
    {
        _objectsByTag.Clear();
        _objectsByType.Clear();
    }

    public static GameObject[] GetGameObjects(uint tag)
    {
        return _objectsByTag[tag].ToArray();
    }

    public static GameObject[] GetGameObjects(string type)
    {
        return _objectsByType[type].ToArray();
    }

    public static uint[] GetTags()
    {
        return _objectsByTag.Keys.ToArray();
    }

    public static string[] GetTypes()
    {
        return _objectsByType.Keys.ToArray();
    }
}
