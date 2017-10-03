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
    [Tooltip("Amount of lines to be parsed per frame.\nLower will reduce Parse speed, higher will freeze the Application.")]
    public uint BatchSize = 2000;

    [Space(20)]
    [Header("Object References")]
    public Material DefaultMaterial;
    public Text ProgressText;
    public Slider ProgressSlider;

    private bool _runInBackground;
    private bool _isGenerated = false;
    private DateTime _startTime;
    private uint _prevBatchSize;

    private long _byteCount = 0;

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
        for (int i = 0; i < Files.Count; ++i)
        {
            if (Files[i].LastIndexOf('.') == -1) Files[i] += ".ifc";
            var fi = new FileInfo(Files[i]);
            _byteCount += fi.Length;
            IFCDataManager.AddDataContainer(fi, new IFCDataContainer(fi, (uint)((float)BatchSize / Files.Count)));
        }

        // Start Coroutines
        //CoroutineManager.Instance().BeginCoroutine(CreateMetadata()); // Debugging purposes
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
            // Clear text
            ProgressText.text = "";
            // Update Parse Progress per parser
            long parsedBytes = 0;
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
                    ProgressText.text +=
                        "[" + container.File.Name + "]: Completed.\n";
                }
                // Parse Progress text
                else
                {
                    var fileName = container.File.Name;
                    ProgressText.text +=
                        "[" + fileName + "]: Parsed " + Helpers.BytesToString(container.GetParsedCharCount()) + "\n";
                }
                parsedBytes += container.GetParsedCharCount();
            }
            // Get elapsed time
            var elapsedSecs = (DateTime.Now - _startTime).TotalMilliseconds / 1000;
            var avgSpeed = (long) (parsedBytes / elapsedSecs);
            var percent = (float) parsedBytes / _byteCount * 100;
            var fps = 1 / Time.deltaTime;
            // Set end text
            ProgressText.text += "\nElapsed Time: " + elapsedSecs.ToString("0") + " seconds.";
            ProgressText.text += "\nAverage speed: " + Helpers.BytesToString(avgSpeed) + "/second. " +
                                 "(FPS: " + fps.ToString("0") + ")";
            ProgressText.text += "\n\nProgress: " + Helpers.BytesToString(parsedBytes) + "/" +
                                 Helpers.BytesToString(_byteCount) +
                                 " (" + percent.ToString("0") + "%)";
            ProgressSlider.value = percent;
            // Wait for next frame
            yield return 0;
        }
        // Hide progress text
        ProgressText.gameObject.SetActive(false);
        ProgressSlider.gameObject.SetActive(false);
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
            Dictionary<uint, GameObject> parents = new Dictionary<uint, GameObject>();
            parents.Add(CreateBuilding(x, scale));
            CreateMeshes(x.GetEntities<IIFCConnectedFaceSet>(true), x, parents);
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

    private KeyValuePair<uint, GameObject> CreateBuilding(IFCDataContainer container, float scale)
    {
        GameObject go = new GameObject();
        var buildings = container.GetEntities<IFCBuilding>(false);
        uint id = 0;
        if (buildings.Count > 0)
        {
            var building = buildings[0];
            go.AddComponent<Metadata>().SetMetadata(building);
            id = building.Id;
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
        return new KeyValuePair<uint, GameObject>(id, go);
    }

    private List<GameObject> CreateBuildingElements(IFCDataContainer container, Dictionary<uint, GameObject> parents)
    {
        List<GameObject> gos = new List<GameObject>();
        GameObject go = new GameObject();
        var elements = container.GetEntities<IIFCBuildingElement>(true);
        return gos;
    }

    #region Mesh Creation
    private void CreateMeshes(List<IIFCConnectedFaceSet> geometrySets, IFCDataContainer container, Dictionary<uint, GameObject> parents)
    {
        foreach (var geometrySet in geometrySets)
            CreateMesh(geometrySet, container, parents);
    }

    private void CreateMesh(IIFCConnectedFaceSet geometrySet, IFCDataContainer container, Dictionary<uint, GameObject> parents)
    {
        // Find correct parrent

        // Create GameObject
        var go = new GameObject();
        go.transform.SetParent(parents.First().Value.transform, false);
        go.name = geometrySet.Id.ToString();

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

        // Add Metadata
        go.AddComponent<Metadata>().SetMetadata(geometrySet);
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
    #endregion
}
