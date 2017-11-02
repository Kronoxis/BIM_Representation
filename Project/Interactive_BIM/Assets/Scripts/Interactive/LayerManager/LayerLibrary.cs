using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class LayerLibrary : MonoBehaviour
{
    public GameObject ModelRoot;
    public GameObject VRFloorRoot;
    public int LayerCount = 0;

    private static Dictionary<string, GameObject> _layers = new Dictionary<string, GameObject>();

    private void Awake()
    {
        Clear();

        var floorLayer = VRFloorRoot.GetComponent<MeshTags>().IfcType;
        if (!_layers.ContainsKey(floorLayer))
            _layers.Add(floorLayer, VRFloorRoot);
        else
            _layers[floorLayer] = VRFloorRoot;

        var model = ModelRoot.transform.GetChild(0);
        for (int i = 0; i < model.childCount; ++i)
        {
            var layer = model.GetChild(i).gameObject;
            if (!_layers.ContainsKey(layer.name))
                _layers.Add(layer.name, layer);
            else
                _layers[layer.name] = layer;
        }
    }

    private void Update()
    {
        LayerCount = _layers.Count;
    }

    public static void AddLayer(string layerName, GameObject parent)
    {
        if (!_layers.ContainsKey(layerName))
            _layers.Add(layerName, parent);
    }

    public static void Clear()
    {
        _layers.Clear();
    }

    public static string[] GetLayerNames()
    {
        return _layers.Keys.ToArray();
    }

    public static GameObject GetLayer(string name)
    {
        if (_layers.ContainsKey(name))
            return _layers[name];
        return null;
    }

    public static GameObject[] GetLayers()
    {
        return _layers.Values.ToArray();
    }
}
