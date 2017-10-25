using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEditor;

[RequireComponent(typeof(MeshRemoveEmpties))]
[RequireComponent(typeof(MeshDoubleFaced))]
[RequireComponent(typeof(MeshTags))]
[RequireComponent(typeof(MeshLayerer))]

[ExecuteInEditMode]
public class ModifyModels : MonoBehaviour
{
    [Header("Model")]
    public GameObject Model;
    [Header("Files")]
    public string DoorPropertiesFilePath;
    [Space(10)]
    [Header("Apply Modifications")]
    public bool Modify = false;

    private void Update()
    {
        if (Modify)
        {
            Modify = false;
            MeshLibrary.Clear();
            if (!Model)
            {
                Debug.Log("No model specified");
                return;
            }
            if (String.IsNullOrEmpty(DoorPropertiesFilePath))
            {
                Debug.Log("No Door Properties File specified");
                return;
            }

            MakeDoubleFaced();
            RemoveEmpties();
            AddTags();
            SeparateLayers();
            CreateDoors();
            Model = null;
        }
    }

    private void RemoveEmpties()
    {
        var script = GetComponent<MeshRemoveEmpties>();
        script.RemoveEmpties(Model);
        Debug.Log(Model.name + " has no more empties");
    }

    private void AddTags()
    {
        var script = GetComponent<MeshTags>();
        foreach (var mesh in Model.GetComponentsInChildren<MeshFilter>())
        {
            script.AddTags(mesh);
        }
        Debug.Log(Model.name + " is now tagged");
    }

    private void MakeDoubleFaced()
    {
        var script = GetComponent<MeshDoubleFaced>();
        foreach (var mesh in Model.GetComponentsInChildren<MeshFilter>())
        {
            script.DoubleObject(mesh.gameObject);
        }
        Debug.Log(Model.name + " is now double faced");
    }

    private void SeparateLayers()
    {
        var script = GetComponent<MeshLayerer>();
        script.Separate(Model);
        Debug.Log(Model.name + " is now layered");
    }

    private void CreateDoors()
    {
        var script = GetComponent<DoorCreator>();
        script.CreateDoors(Model, DoorPropertiesFilePath);
        Debug.Log(Model.name + " now has doors");
    }
}
