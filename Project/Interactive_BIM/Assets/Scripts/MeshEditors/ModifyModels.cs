using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshRemoveEmpties))]
[RequireComponent(typeof(MeshDoubleFaced))]
[RequireComponent(typeof(MeshTags))]
[RequireComponent(typeof(MeshLayerer))]

[ExecuteInEditMode]
public class ModifyModels : MonoBehaviour
{
    [Header("Models")]
    public List<GameObject> Models = new List<GameObject>();
    [Header("Modifications")]
    public bool Tags = false;
    public bool Layers = false;
    public bool Unclutter = false;
    [Tooltip("Make sure model isn't already double faced before making it double faced!")]
    public bool DoubleFaced = false;
    public bool Doors = false;
    [Header("Files")]
    public List<string> DoorPropertiesFilePath = new List<string>();
    [Space(10)]
    [Header("Apply Modifications")]
    public bool Modify = false;

    private void Update()
    {
        if (Modify)
        {
            if (Unclutter) RemoveEmpties();
            if (Tags) AddTags();
            if (Layers) SeparateLayers();
            if (DoubleFaced) MakeDoubleFaced();
            if (Doors) CreateDoors();
            Models.Clear();
            Modify = false;
        }
    }

    private void RemoveEmpties()
    {
        var script = GetComponent<MeshRemoveEmpties>();
        foreach (var model in Models)
        {
            script.RemoveEmpties(model);
            Debug.Log(model.name + " has no more empties");
        }
        Unclutter = false;
    }

    private void AddTags()
    {
        var script = GetComponent<MeshTags>();
        foreach (var model in Models)
        {
            foreach (var mesh in model.GetComponentsInChildren<MeshFilter>())
            {
                script.AddTags(mesh);
            }
            Debug.Log(model.name + " is now tagged");
        }
        Tags = false;
    }

    private void MakeDoubleFaced()
    {
        var script = GetComponent<MeshDoubleFaced>();
        foreach (var model in Models)
        {
            foreach (var mesh in model.GetComponentsInChildren<MeshFilter>())
            {
                script.DoubleFace(mesh);
            }
            Debug.Log(model.name + " is now double faced");
        }
        DoubleFaced = false;
    }

    private void SeparateLayers()
    {
        var script = GetComponent<MeshLayerer>();
        foreach (var model in Models)
        {
            script.Separate(model);
            Debug.Log(model.name + " is now layered");
        }
        Layers = false;
    }

    private void CreateDoors()
    {
        var script = GetComponent<DoorCreator>();
        int i = 0;
        foreach (var model in Models)
        {
            if (DoorPropertiesFilePath.Count <= i) --i;
            script.CreateDoors(model, DoorPropertiesFilePath[i]);
            ++i;
            Debug.Log(model.name + " now has doors");
        }
        Doors = false;
    }
}
