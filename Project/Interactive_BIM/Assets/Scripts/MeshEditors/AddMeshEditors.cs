using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMeshEditors : MonoBehaviour
{
    public GameObject MeshEditorScripts;

	private void Awake()
	{
	    var scripts = MeshEditorScripts.GetComponents<MeshEditor>();
        foreach (var child in GetComponentsInChildren<Transform>())
        {
            foreach (var script in scripts)
            {
                Type t = script.GetType();
                child.gameObject.AddComponent(t);
            }
        }
	}
}
