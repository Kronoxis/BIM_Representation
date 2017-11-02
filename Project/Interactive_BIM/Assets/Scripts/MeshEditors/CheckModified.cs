using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CheckModified : MonoBehaviour
{
    private void Update()
    {
        for (int i = 0; i < transform.childCount; ++i)
        {
            if (!transform.GetChild(i).GetComponent<TagModified>())
                MeshLibrary.Clear();
        }
    }
}
