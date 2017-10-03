using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class IFCBuildingGenerator : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Add Files to Parse.\nRequires Full Path.\nIncrease the Size to add more files.")]
    public List<string> Files = new List<string>() { "" };
    [Space(10)]
    [Tooltip("Amount of lines to be parsed per frame.\nRecommend 500-2500.\nLower will reduce Parse speed, higher will freeze the Application.")]
    public uint BatchSize = 1000;

    [Space(20)]
    [Header("Object References")]
    public Material DefaultMaterial;
    public Text ParseProgress;

    //private List<IFCDataContainer> _dataContainers = new List<IFCDataContainer>();

    private bool _runInBackground;
    private bool _isGenerated = false;
    private DateTime _startTime;
    private uint _prevBatchSize;

    private void Awake()
    {
        _startTime = DateTime.Now;
        _prevBatchSize = BatchSize;
    }
    // Use this for initialization
    private void Start()
    {
        _runInBackground = Application.runInBackground;
        Application.runInBackground = true;
        foreach (var file in Files)
        {
            var container = new IFCDataContainer(file, (uint)((float)BatchSize / Files.Count));
            IFCDataManager.AddDataContainer(file, container);
            //CoroutineManager.Instance().BeginCoroutine(CreateGeometry(container));

        }
        //CoroutineManager.Instance().BeginCoroutine(CreateMetadata());
        CoroutineManager.Instance().BeginCoroutine(CreateGeometry());
        CoroutineManager.Instance().BeginCoroutine(UpdateParseProgress());
    }

    private void Update()
    {
        if (!_isGenerated && _prevBatchSize != BatchSize)
        {
            IFCDataManager.GetAllData().ForEach(c => c.SetBatchSize(BatchSize));
            _prevBatchSize = BatchSize;
        }
    }

    private IEnumerator UpdateParseProgress()
    {
        int wpc;
        int prevWpc = IFCDataManager.GetWorkingParserCount();
        while (IFCDataManager.IsParsing(out wpc))
        {
            var totalLines = 0;
            // Clear text
            ParseProgress.text = "";
            // Update Parse Progress per parser
            foreach (var container in IFCDataManager.GetAllData())
            {
                // Update Batch Size
                if (prevWpc != wpc)
                {
                    container.SetBatchSize((uint)(BatchSize / wpc));
                    prevWpc = wpc;
                }
                // Completed text
                if (container.IsParsed())
                {
                    ParseProgress.text +=
                        "[" + IFCDataManager.GetContainerFile(container) + "]: Completed.\n";
                }
                // Parse Progress text
                else
                {
                    ParseProgress.text +=
                        "[" + IFCDataManager.GetContainerFile(container) + "]: Parsed " + container.GetParsedLineCount() + " lines...\n";
                }
                totalLines += container.GetParsedLineCount();
            }
            // Get elapsed time
            var elapsedSecs = (DateTime.Now - _startTime).TotalMilliseconds / 1000;
            // Set end text
            ParseProgress.text += "Elapsed Time: " + elapsedSecs.ToString("0.000") + " seconds.\n";
            ParseProgress.text += "FPS: " + (1 / Time.deltaTime).ToString("0") + "\n";
            ParseProgress.text += "Average speed: " + (totalLines / elapsedSecs).ToString("0") + " lines/second.\n";
            // Wait for next frame
            yield return 0;
        }
        // Hide progress text
        ParseProgress.enabled = false;
        // Reset runInBackground 
        Application.runInBackground = _runInBackground;
        // Flag building generated
        _isGenerated = true;
    }

    private IEnumerator CreateMetadata()
    {
        while (IFCDataManager.IsParsing()) yield return 0;

        IFCDataManager.GetAllData().ForEach(x =>
        {
            x.GetEntities<IFCEntity>(true).ForEach(e =>
            {
                GameObject go = new GameObject();
                go.name = e.GetType().ToString() + " #" + e.Id.ToString();
                var script = go.AddComponent<Metadata>();
                script.SetMetadata(e);
            });
        });
    }

    private IEnumerator CreateGeometry()
    {
        while (IFCDataManager.IsParsing()) yield return 0;

        IFCDataManager.GetAllData().ForEach(x =>
        {
            var scale = GetScale(x);
            var building = CreateBuilding(x, scale);
            CreateMeshes(x.GetEntities<IIFCConnectedFaceSet>(true), x, building);
        });
    }

    private float GetScale(IFCDataContainer container)
    {
        float scale = 1;
        var units = container.GetEntities<IIFCNamedUnit>(true);
        var lengthUnits = units.Where(unit => unit.GetProperty("UnitType").AsEnum<IFCUnitEnum>() == IFCUnitEnum.LENGTHUNIT).ToArray();
        if (lengthUnits.Length > 0)
        {
            var pow = (int)lengthUnits[0].GetProperty("Prefix").AsEnum<IFCSIPrefix>();
            scale = Mathf.Pow(10, pow);
        }
        return scale;
    }

    private GameObject CreateBuilding(IFCDataContainer container, float scale)
    {
        GameObject go = new GameObject();
        var buildings = container.GetEntities<IFCBuilding>(false);
        if (buildings.Count > 0)
        {
            var building = buildings[0];
            go.name = building.GetProperty("Name").AsString() + " (" + building.GetProperty("GlobalId").AsString() +
                      ")";
            var coords = building.GetReference("ObjectPlacement").GetReference("RelativePlacement")
                .GetReference("Location").GetProperty("Coordinates").AsList();
            Vector3 pos = new Vector3(coords[0].AsFloat(), coords[1].AsFloat(), coords[2].AsFloat());
            go.transform.Translate(pos);
        }
        else
        {
            go.name = "Missing IFCBuilding!";
        }
        // Resolve misaligned axis (Z-up instead of Y-up)
        go.transform.Rotate(new Vector3(-90, 0, 0));
        // Set Scale
        go.transform.localScale = new Vector3(scale, scale, scale);
        return go;
    }

    //private IEnumerator CreateGeometry(IFCDataContainer container)
    //{
    //    // Wait until parsed
    //    while (!container.IsParsed()) yield return 0;
    //
    //    // Calculate size
    //    var scale = GetScale(container);
    //
    //    // Create Building
    //    var building = CreateBuilding(container, scale);
    //
    //    // Create Building Elements
    //    var buildingElements = CreateBuildingElements(container, building);
    //
    //    // Get Geometry Sets
    //    var geometrySets = container.GetEntities<IIFCConnectedFaceSet>(true);
    //
    //    // Create meshes from geometry sets
    //    CreateMeshes(geometrySets, container, building);
    //}

    //private float GetScale(IFCDataContainer container)
    //{
    //    float scale = 1;
    //    var units = container.GetEntities<IIFCNamedUnit>(true);
    //    //var lengthUnits = units.Where(unit => unit.GetEnumProperty<IFCUnitEnum>("UnitType") == IFCUnitEnum.LENGTHUNIT).ToArray();
    //    //if (lengthUnits.Length > 0)
    //    //{
    //    //    var pow = (int)lengthUnits[0].GetEnumProperty<IFCSIPrefix>("Prefix");
    //    //    scale = Mathf.Pow(10, pow);
    //    //}
    //    return scale;
    //}

    //private GameObject CreateBuilding(IFCDataContainer container, float scale)
    //{
    //    GameObject go = new GameObject();
    //    var buildings = container.GetEntities<IFCBuilding>(false);
    //    if (buildings.Count > 0)
    //    {
    //        var building = buildings[0];
    //        //go.name = building.GetStringProperty("Name") + " (" + building.GetStringProperty("GlobalId") + ")";
    //        //var location = building.GetReference<IFCCartesianPoint>(new [] {"ObjectPlacement", "RelativePlacement", "Location"});
    //        //go.transform.Translate(location.GetVector3());
    //    }
    //    else
    //    {
    //        go.name = "Missing IFCBuilding!";
    //    }
    //    // Resolve misaligned axis
    //    go.transform.Rotate(new Vector3(-90, 0, 0));
    //    // Set Scale
    //    go.transform.localScale = new Vector3(scale, scale, scale);
    //    return go;
    //}

    private List<GameObject> CreateBuildingElements(IFCDataContainer container, GameObject parent)
    {
        List<GameObject> gos = new List<GameObject>();
        GameObject go = new GameObject();
        var elements = container.GetEntities<IIFCBuildingElement>(true);
        return gos;
    }

    private void CreateMeshes(List<IIFCConnectedFaceSet> geometrySets, IFCDataContainer container, GameObject parent)
    {
        foreach (var geometrySet in geometrySets)
            CreateMesh(geometrySet, container, parent);
    }

    private void CreateMesh(IIFCConnectedFaceSet geometrySet, IFCDataContainer container, GameObject parent)
    {
        // Create GameObject
        var go = new GameObject();
        go.transform.SetParent(parent.transform, false);
        go.name = geometrySet.Id.ToString();
        go.AddComponent<Metadata>().SetMetadata(geometrySet);

        // Add Mesh Filter
        var mf = go.AddComponent<MeshFilter>();
        // Set Buffers
        List<Vector3> vertices;
        List<int> indices;
        List<Vector3> normals;
        List<Vector2> uvs;
        CreateBuffers(geometrySet, out vertices, out indices, out normals, out uvs);
        mf.mesh.vertices = vertices.ToArray();
        mf.mesh.triangles = indices.ToArray();
        mf.mesh.normals = normals.ToArray();
        mf.mesh.uv = uvs.ToArray();

        // Add Mesh Renderer
        var mr = go.AddComponent<MeshRenderer>();
        // Set Material
        mr.material = DefaultMaterial;
    }

    public void CreateBuffers(IIFCConnectedFaceSet geometrySet,
         out List<Vector3> vertices, out List<int> indices, out List<Vector3> normals, out List<Vector2> uvs)
    {
        vertices = new List<Vector3>();
        indices = new List<int>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();

        var container = IFCDataManager.GetDataContainer(geometrySet.File);
        foreach (var face in geometrySet.GetProperty("CfsFaces").AsList())
        {
            foreach (var bound in container.GetEntity(face.AsId()).GetProperty("Bounds").AsList())
            {
                var loopId = container.GetEntity(bound.AsId()).GetProperty("Bound").AsId();
                List<Vector3> vertsFromLoop = new List<Vector3>();
                foreach (var point in container.GetEntity(loopId).GetProperty("Polygon").AsList())
                {
                    var coords = container.GetEntity(point.AsId()).GetProperty("Coordinates").AsList();
                    vertsFromLoop.Add(new Vector3(coords[0].AsFloat(), coords[1].AsFloat(), coords[2].AsFloat()));
                }
                AddToBuffer(vertsFromLoop, ref vertices, ref indices, ref normals, ref uvs);
            }
        }
    }

    private void AddToBuffer(List<Vector3> verts,
        ref List<Vector3> vertices, ref List<int> indices, ref List<Vector3> normals, ref List<Vector2> uvs)
    {
        List<int> vertRefs = new List<int>();
        int index = indices.Count;
        Vector3 normal = CalculateNormal(verts[0], verts[1], verts[2]);
        Vector2 uv = new Vector2(0, 0);

        for (int i = 2; i < verts.Count; ++i)
        {
            vertRefs.Add(0);
            vertRefs.Add(i - 1);
            vertRefs.Add(i);
        }

        for (int i = 0; i < vertRefs.Count; ++i)
        {
            AddToBuffer(verts[vertRefs[i]], index + i, normal, uv,
                ref vertices, ref indices, ref normals, ref uvs);
        }
    }

    private void AddToBuffer(Vector3 vert, int index, Vector3 normal, Vector2 uv,
        ref List<Vector3> vertices, ref List<int> indices, ref List<Vector3> normals, ref List<Vector2> uvs)
    {
        vertices.Add(vert);
        indices.Add(index);
        normals.Add(normal);
        uvs.Add(uv);
    }

    private Vector3 CalculateNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        return Vector3.Cross(b - a, c - a).normalized;
    }
}
