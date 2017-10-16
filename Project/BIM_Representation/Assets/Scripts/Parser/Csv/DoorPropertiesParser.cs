using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DoorPropertiesParser : MonoBehaviour
{
    public string Path;

    private void Start()
    {
        CsvParser parser = new CsvParser();
        parser.Parse(Path);
        var keys = parser.Keys;
        var values = parser.Values;

        GameObject parent = new GameObject();
        parent.name = "DoorProperties";

        foreach (var valueList in values)
        {
            GameObject go = new GameObject();
            go.AddComponent<MeshTag>();
            var properties = go.AddComponent<PropertiesContainer>();
            properties.CreateProperties(keys.ToArray(), valueList);
            var tag = uint.Parse(properties["Tag"]);
            go.name = "DoorProperties:" + tag;
            go.transform.SetParent(parent.transform);
            go.transform.position = new Vector3(
                -float.Parse(properties["CenterX"]),
                -float.Parse(properties["CenterZ"]),
                -float.Parse(properties["CenterY"])
            );
            var startAngle = float.Parse(properties["StartAngle"]);
            var sweepAngle = float.Parse(properties["SweepAngle"]);
            AddDoorScript(tag, go, startAngle, sweepAngle);
        }
    }

    private void AddDoorScript(uint tag, GameObject pivot, float startAngle, float sweepAngle)
    {
        pivot.AddComponent<Door>().Set(pivot, startAngle, sweepAngle);
        pivot.AddComponent<Rigidbody>().isKinematic = true;
        foreach (var go in MeshLibrary.GetGameObjects(tag))
        {
            var renderer = go.GetComponent<MeshRenderer>();
            if (renderer && !renderer.material.name.Contains("Frame"))
                go.transform.SetParent(pivot.transform);
        }
    }
}
