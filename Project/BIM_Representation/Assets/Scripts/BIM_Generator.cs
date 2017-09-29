using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BIM_Generator : MonoBehaviour
{
    /*public string FilePath;
    public Material DefaultMaterial;
    public uint BatchSize = 500;

    private BIM_Parser _parser;
    private Dictionary<uint, IfcTopologicalRepresentationItem> _items = new Dictionary<uint, IfcTopologicalRepresentationItem>();
    private bool _isParsed = false;

    private int _parsedLineCount = 0;

    private void Start ()
	{
	    if (string.IsNullOrEmpty(FilePath))
	    {
	        Debug.LogError("Please specify a FilePath");
            return;
	    }

	    HeaderBlock header;
	    _parser = new BIM_Parser(FilePath, out header);
	    if (!_parser.IsValid()) return;
	    StartCoroutine(ReadData());
	    StartCoroutine(CreateMeshes());
	}

    private IEnumerator ReadData()
    {
        Debug.Log("Parsing Data of " + FilePath);
        while (!_parser.IsEnd())
        {
            //var kvItem = _parser.GetNextIfcTopologicalRepresentationItem();
            //if (kvItem.Value == IfcTopologicalRepresentationItems.IfcClosedShell)
            //{
            //    CreateMesh(kvItem.Key);
            //}
            var item = _parser.GetNextIfcTopologicalRepresentationItem();
            if (item.GetType() != typeof(IfcNull))
            {
                _items.Add(item.Id, item);
            }
            ++_parsedLineCount;
            Debug.Log("Parsed " + _parsedLineCount + " lines from file " + FilePath);
            if (_parsedLineCount % BatchSize == 0) yield return 0;
        }
        _isParsed = true;
        Debug.Log("Parsed all Data of " + FilePath);
    }

    private IEnumerator CreateMeshes()
    {
        while (!_isParsed) yield return 0;
        Debug.Log("Creating Meshes of " + FilePath);
        var shells = GetItems<IfcClosedShell>();
        foreach (var shell in shells)
        {
            CreateMesh(shell.Id);
            yield return 0;
        }
        Debug.Log("Created Meshes of " + FilePath);
    }

    private void CreateMesh(uint id)
    {
        CreateMesh(GetItem<IfcConnectedFaceSet>(id));
    }

    private void CreateMesh(IfcConnectedFaceSet shell)
    {
        var go = new GameObject();
        go.name = shell.Id.ToString();
        var mf = go.AddComponent<MeshFilter>();
        mf.mesh.vertices = shell.GetVertices(this).ToArray();
        mf.mesh.triangles = shell.GetIndices(this).ToArray();
        var mr = go.AddComponent<MeshRenderer>();
        mr.material = DefaultMaterial;
        go.transform.localScale.Set(0.001f, 0.001f, 0.001f);
    }

    public T GetItem<T>(uint id) where T : IfcTopologicalRepresentationItem
    {
        return _items[id] as T;
    }

    public List<T> GetItems<T>(List<uint> ids) where T : IfcTopologicalRepresentationItem
    {
        List<T> items = new List<T>();
        foreach (var id in ids)
        {
            items.Add(GetItem<T>(id));
        }
        return items;
    }

    public List<T> GetItems<T>() where T : IfcTopologicalRepresentationItem
    {
        List<T> items = new List<T>();
        foreach (var item in _items)
        {
            Debug.Log(item.Value.GetType());
            if (item.Value.GetType() == typeof(T))
            {
                items.Add(item.Value as T);
            }
        }
        return items;
    }
    */
}
