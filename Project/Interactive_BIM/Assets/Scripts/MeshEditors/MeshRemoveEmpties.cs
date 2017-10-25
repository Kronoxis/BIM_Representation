using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
public class MeshRemoveEmpties : MonoBehaviour
{
    public void RemoveEmpties(GameObject go)
    {
        List<GameObject> toDestroy = new List<GameObject>();
        foreach (var child in go.GetComponentsInChildren<Transform>())
        {
            if (child == go.transform) continue;
            child.SetParent(go.transform);
            if (child.GetComponent<MeshFilter>()) continue;
            toDestroy.Add(child.gameObject);
        }
        toDestroy.ForEach(x => DestroyImmediate(x));
    }
}
