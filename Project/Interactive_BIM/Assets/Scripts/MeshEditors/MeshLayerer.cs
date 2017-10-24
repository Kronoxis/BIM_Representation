using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshLayerer : MonoBehaviour
{
    public void Separate(GameObject model)
    {
        foreach (var type in MeshLibrary.GetTypes())
        {
            var layer = model.transform.Find(type);
            if (!layer) layer = new GameObject().transform;
            layer.SetParent(model.transform);
            layer.name = type;
            foreach (var go in MeshLibrary.GetGameObjects(type))
            {
                if (go.GetComponent<MeshRenderer>())
                    go.transform.SetParent(layer);
            }
        }
    }
}
