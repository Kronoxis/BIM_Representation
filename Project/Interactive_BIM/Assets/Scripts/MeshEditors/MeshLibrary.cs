using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode, System.Serializable]
public class MeshLibrary : MonoBehaviour
{
    public GameObject ModelRoot;
    public int MeshCount = 0;

    private static MultiValueDictionary<uint, GameObject> _objectsByTag =
        new MultiValueDictionary<uint, GameObject>();
    private static MultiValueDictionary<string, GameObject> _objectsByType =
        new MultiValueDictionary<string, GameObject>();

    private void Awake()
    {
        Clear();
        ModelRoot.GetComponentsInChildren<MeshFilter>().ToList()
            .ForEach(x =>
            {
                var tags = x.GetComponent<MeshTags>();
                AddGameObject(tags.Tag, tags.IfcType, x.gameObject);
            });
    }

    private void Update()
    {
        MeshCount = _objectsByTag.Count;
    }

    public static void AddGameObject(uint tag, string type, GameObject go)
    {
        // Tag
        if (!_objectsByTag.ContainsValue(tag, go))
            _objectsByTag.Add(tag, go);

        // Type
        if (!_objectsByType.ContainsValue(type, go))
            _objectsByType.Add(type, go);
    }

    public static void Clear()
    {
        _objectsByTag.Clear();
        _objectsByType.Clear();
    }

    public static GameObject[] GetGameObjects(uint tag)
    {
        if (!_objectsByTag.ContainsKey(tag)) Debug.Log("MeshLibrary doesn't contain " + tag);
        else return _objectsByTag[tag].ToArray();
        return null;
    }

    public static GameObject[] GetGameObjects(string type)
    {
        if (!_objectsByType.ContainsKey(type)) Debug.Log("MeshLibrary doesn't contain " + type);
        else return _objectsByType[type].ToArray();
        return null;
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