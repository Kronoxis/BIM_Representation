using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DoorCreator : MonoBehaviour
{
    public void CreateDoors(string path)
    {
        // Parse CSV file
        CsvParser parser = new CsvParser();
        parser.Parse(path);
        var keys = parser.Keys;
        var values = parser.Values;

        foreach (var valueList in values)
        {
            // Create door container
            GameObject go = new GameObject();

            // Add properties
            var properties = go.AddComponent<PropertiesContainer>();
            properties.CreateProperties(keys.ToArray(), valueList);

            // Set name, parent, position
            var ifcTag = uint.Parse(properties["Tag"]);
            go.name = "DoorPivot:" + ifcTag;
            var parent = MeshLibrary.GetGameObjects(ifcTag)[0].transform.parent;
            go.transform.SetParent(parent);
            go.transform.localPosition = new Vector3(
                -float.Parse(properties["CenterX"]),
                -float.Parse(properties["CenterZ"]),
                -float.Parse(properties["CenterY"])
            );
            
            // Get angles
            var startAngle = float.Parse(properties["StartAngle"]);
            var sweepAngle = float.Parse(properties["SweepAngle"]);

            // Add door script 
            AddDoorScript(ifcTag, go, startAngle, sweepAngle);
        }
    }

    private void AddDoorScript(uint ifcTag, GameObject pivot, float startAngle, float sweepAngle)
    {
        pivot.AddComponent<Door>().Set(pivot, startAngle, sweepAngle);
        pivot.AddComponent<Rigidbody>().isKinematic = true;
        var gos = MeshLibrary.GetGameObjects(ifcTag);
        foreach (var go in gos)
        {
            var ren = go.GetComponent<MeshRenderer>();
            if (ren && !ren.sharedMaterial.name.Contains("Frame"))
                go.transform.SetParent(pivot.transform);
        }
    }
}
