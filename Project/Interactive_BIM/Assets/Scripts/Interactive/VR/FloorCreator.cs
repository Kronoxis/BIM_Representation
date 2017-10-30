using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCreator : MonoBehaviour
{
    public void CreateFloors(string[] types)
    {
        foreach (var type in types)
        {
            string ifcType = type.Contains("Ifc") ? type : "Ifc" + type;
            var gos = MeshLibrary.GetGameObjects(ifcType);
            if (gos == null) continue;
            foreach (var go in gos)
            {
                go.layer = LayerMask.NameToLayer("CanTeleport");
            }
        }
    }
}
