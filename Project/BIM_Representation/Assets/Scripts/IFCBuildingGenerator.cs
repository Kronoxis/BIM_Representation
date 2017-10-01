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
    public List<string> Files = new List<string>();
    public uint BatchSize = 1000;
    public Material DefaultMaterial;
    public Text ParseProgress;

    private List<IFCDataContainer> _dataContainers = new List<IFCDataContainer>();

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
            _dataContainers.Add(container);
            CoroutineManager.Instance().BeginCoroutine(CreateGeometry(container));

        }
        CoroutineManager.Instance().BeginCoroutine(UpdateParseProgress(_dataContainers));
    }

    private void Update()
    {
        if (!_isGenerated && _prevBatchSize != BatchSize)
        {
            _dataContainers.ForEach(c => c.SetBatchSize(BatchSize));
            _prevBatchSize = BatchSize;
        }
    }

    private IEnumerator UpdateParseProgress(List<IFCDataContainer> containers)
    {
        // File names (Info)
        List<string> fileNames = new List<string>();
        Files.ForEach(f => fileNames.Add(new FileInfo(f).Name));

        var workingParserCount = GetWorkingParsersCount(containers);
        var prevWorkingParserCount = workingParserCount;
        while (workingParserCount > 0)
        {
            prevWorkingParserCount = workingParserCount;
            var totalLines = 0;
            // Clear text
            ParseProgress.text = "";
            // Update Parse Progress per parser
            for (int i = 0; i < containers.Count; ++i)
            {
                var container = containers[i];
                // Update Batch Size
                if (prevWorkingParserCount != workingParserCount)
                    container.SetBatchSize((uint)(BatchSize / workingParserCount));
                // Completed text
                if (container.IsParsed())
                {
                    ParseProgress.text +=
                        "[" + fileNames[i] + "]: Completed.\n";
                }
                // Parse Progress text
                else
                {
                    ParseProgress.text +=
                        "[" + fileNames[i] + "]: Parsed " + container.GetParsedLineCount() + " lines...\n";
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
            workingParserCount = GetWorkingParsersCount(containers);
        }
        // Hide progress text
        ParseProgress.enabled = false;
        // Reset runInBackground 
        Application.runInBackground = _runInBackground;
        // Flag building generated
        _isGenerated = true;
    }

    private int GetWorkingParsersCount(List<IFCDataContainer> containers)
    {
        return containers.Where(c => !c.IsParsed()).ToArray().Length;
    }

    private IEnumerator CreateGeometry(IFCDataContainer container)
    {
        // Wait until parsed
        while (!container.IsParsed()) yield return 0;

        // Calculate size
        float scale = 1;
        var units = container.GetEntities<IIFCNamedUnit>(true);
        var lengthUnits = units.Where(unit => unit.GetEnumProperty<IFCUnitEnum>("UnitType") == IFCUnitEnum.LENGTHUNIT).ToArray();
        if (lengthUnits.Length > 0)
        {
            var pow = (int)lengthUnits[0].GetEnumProperty<IFCSIPrefix>("Prefix");
            scale = Mathf.Pow(10, pow);
        }

        // Get Geometry Sets
        var geometrySets = container.GetEntities<IIFCConnectedFaceSet>(true);

        // Create meshes from geometry sets
        CreateMeshes(geometrySets, container, scale);
    }

    private void CreateMeshes(List<IIFCConnectedFaceSet> geometrySets, IFCDataContainer container, float scale)
    {
        foreach (var geometrySet in geometrySets)
            CreateMesh(geometrySet, container, scale);
    }

    private void CreateMesh(IIFCConnectedFaceSet geometrySet, IFCDataContainer container, float scale)
    {
        // Create GameObject
        var go = new GameObject();
        go.name = geometrySet.Id.ToString();
        // Resolve misaligned axis
        go.transform.Rotate(new Vector3(-90, 0, 0));
        // Set Position
        // Set Rotation
        // Set Scale
        go.transform.localScale = new Vector3(scale, scale, scale);

        // Add Mesh Filter
        var mf = go.AddComponent<MeshFilter>();
        // Set Buffers
        List<Vector3> vertices;
        List<int> indices;
        List<Vector3> normals;
        List<Vector2> uvs;
        geometrySet.GetMeshFilterBuffers(container, out vertices, out indices, out normals, out uvs);
        mf.mesh.vertices = vertices.ToArray();
        mf.mesh.triangles = indices.ToArray();
        mf.mesh.normals = normals.ToArray();
        mf.mesh.uv = uvs.ToArray();

        // Add Mesh Renderer
        var mr = go.AddComponent<MeshRenderer>();
        // Set Material
        mr.material = DefaultMaterial;
    }
}
