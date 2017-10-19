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
                        "[" + fileName + "]: Parsed " + IfcHelpers.BytesToString(container.GetParsedCharCount()) + "\n";
                }
                parsedBytes += container.GetParsedCharCount();
            }
            // Get elapsed time
            var elapsedSecs = (DateTime.Now - _startTime).TotalMilliseconds / 1000;
            var avgSpeed = (long)(parsedBytes / elapsedSecs);
            var percent = (float)parsedBytes / _byteCount * 100;
            var fps = 1 / Time.deltaTime;
            // Set end text
            ProgressText.text += "\nElapsed Time: " + elapsedSecs.ToString("0") + " seconds.";
            ProgressText.text += "\nAverage speed: " + IfcHelpers.BytesToString(avgSpeed) + "/second. " +
                                 "(FPS: " + fps.ToString("0") + ")";
            ProgressText.text += "\n\nProgress: " + IfcHelpers.BytesToString(parsedBytes) + "/" +
                                 IfcHelpers.BytesToString(_byteCount) +
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

        IFCDataManager.GetAllData().ForEach(container =>
        {
            var goBuilding = CreateBuilding(container);
            foreach (var element in container.GetEntities<IIFCBuildingElement>(true))
            {
                var goBuildingElement = CreateBuildingElement(element, goBuilding.transform);
                var buildingElementRepresentation = element.GetReference("Representation");
                if (buildingElementRepresentation == null) continue;
                var buildingElementRepresentations = buildingElementRepresentation.GetReferences<IIFCRepresentation>("Representations");
                foreach (var representation in buildingElementRepresentations)
                {
                    var goRepresentation = CreateRepresentation(representation, goBuildingElement.transform);
                    var items = representation.GetReferences<IIFCRepresentationItem>("Items");
                    foreach (var item in items)
                    {
                        if (item.Is<IIFCManifoldSolidBrep>(true))
                        {
                            CreateMeshFromBrep((IIFCManifoldSolidBrep)item, goRepresentation.transform);
                        }
                        else if (item.Is<IFCMappedItem>(true))
                        {
                            
                            var source = item.GetReference<IFCRepresentationMap>("MappingSource");
                            var mappedRepresentationItems = source.GetReference<IIFCRepresentation>("MappedRepresentation")
                                .GetReferences<IIFCRepresentationItem>("Items");
                            foreach (var mappedRepresentationItem in mappedRepresentationItems)
                                CreateRepresentationItem(mappedRepresentationItem, goRepresentation.transform, container);
                        }
                    }
                }
            }
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

    private GameObject CreateGameObject(IFCEntity e, string goName, Transform parent, Vector3 pos, Vector3 rot, float scale)
    {
        GameObject go = new GameObject();
        go.AddComponent<Metadata>().SetMetadata(e);
        go.name = goName;
        go.transform.SetParent(parent, false);
        go.transform.Translate(pos);
        go.transform.Rotate(rot);
        go.transform.localScale.Set(scale, scale, scale);
        return go;
    }

    #region Layer Creation
    private GameObject CreateBuilding(IFCDataContainer container)
    {
        var buildings = container.GetEntities<IFCBuilding>(false);
        IFCBuilding building = null;
        var goName = "";
        var pos = new Vector3(0, 0, 0);
        var rot = new Vector3(-90, 0, 0); // Resolve misaligned axis (Z-up instead of Y-up)
        var scale = GetScale(container);

        if (buildings.Count > 0)
        {
            building = buildings[0];
            goName = building.GetProperty("Name").AsString() + " (" + building.GetProperty("GlobalId").AsString() +
                     ")";
            var coords = building.GetReference("ObjectPlacement").GetReference("RelativePlacement")
                .GetReference("Location").GetProperty("Coordinates").AsList();
            pos = new Vector3(coords[0].AsFloat(), coords[1].AsFloat(), coords[2].AsFloat());
        }
        else
        {
            goName = "Missing IFCBuilding!";
        }
        return CreateGameObject(building, goName, null, pos, rot, scale);
    }

    private GameObject CreateBuildingElement(IIFCBuildingElement element, Transform parent)
    {
        var globalId = element.GetProperty("GlobalId").AsString();
        var elemName = element.GetProperty("Name").AsString();
        var goName = elemName + " (" + globalId + ")";
        var coords = element.GetReference("ObjectPlacement").GetReference("RelativePlacement")
            .GetReference("Location").GetProperty("Coordinates").AsList();
        Vector3 pos = new Vector3(coords[0].AsFloat(),
            coords[1].AsFloat(), coords[2].AsFloat());
        return CreateGameObject(element, goName, parent, pos, new Vector3(0, 0, 0), 1);
    }

    private GameObject CreateRepresentation(IIFCRepresentation representation, Transform parent)
    {
        var context = representation.GetReference("ContextOfItems");
        while (context.Is<IFCGeometricRepresentationSubContext>(false))
            context = context.GetReference("ParentContext");
        var axis = context.GetReference("WorldCoordinateSystem");
        var coords = axis.GetReference("Location").GetProperty("Coordinates").AsList();
        var pos = new Vector3(coords[0].AsFloat(), coords[1].AsFloat(), coords[2].AsFloat());
        var idName = representation.GetProperty("RepresentationIdentifier").AsString();
        var type = representation.GetProperty("RepresentationType").AsString();
        var goName = idName + " (" + type + ")";
        return CreateGameObject(representation, goName, parent, pos, new Vector3(0, 0, 0), 1);
    }

    private GameObject CreateRepresentationItem(IIFCRepresentationItem representationItem, Transform parent, IFCDataContainer container)
    {
        var go = new GameObject();
        go.transform.SetParent(parent, false);
        go.name = representationItem.Id.ToString();

        if (representationItem.Is<IFCFaceBasedSurfaceModel>(true))
            CreateMeshFromFaceSets(
                representationItem.GetReferences<IFCConnectedFaceSet>("FbsmFaces").ToArray(), go.transform);
        
        else if (representationItem.Is<IFCShellBasedSurfaceModel>(true))
            CreateMeshFromFaceSets(
                representationItem.GetReferences<IFCConnectedFaceSet>("SbsmFaces").ToArray(), go.transform);

        return go;
    }
    #endregion

    #region Mesh Creation
    private void CreateMeshFromBrep(IIFCManifoldSolidBrep brep, Transform parent)
    {
        CreateMeshFromFaceSet(brep.GetReference<IFCConnectedFaceSet>("Outer"), parent);
    }

    private void CreateMeshFromFaceSets(IFCConnectedFaceSet[] faceSets, Transform parent)
    {
        foreach (var faceSet in faceSets)
        {
            // Create Mesh
            CreateMeshFromFaceSet(faceSet, parent);
        }
    }

    private void CreateMeshFromFaceSet(IFCConnectedFaceSet faceSet, Transform parent)
    {
        // Create GameObject
        var go = new GameObject();
        go.transform.SetParent(parent, false);
        go.name = faceSet.Id.ToString();

        // Add Mesh Filter
        var mf = go.AddComponent<MeshFilter>();
        // Set Buffers
        List<Vector3> vertices;
        List<int> indices;
        List<Vector3> normals;
        List<Vector2> uvs;
        CreateBuffers(faceSet, out vertices, out indices, out normals, out uvs);
        mf.mesh.vertices = vertices.ToArray();
        mf.mesh.triangles = indices.ToArray();
        mf.mesh.normals = normals.ToArray();
        mf.mesh.uv = uvs.ToArray();

        // Add Mesh Renderer
        var mr = go.AddComponent<MeshRenderer>();
        // Set Material
        mr.material = DefaultMaterial;

        // Add Metadata
        go.AddComponent<Metadata>().SetMetadata(faceSet);
    }

    public void CreateBuffers(IFCConnectedFaceSet geometrySet,
         out List<Vector3> vertices, out List<int> indices, out List<Vector3> normals, out List<Vector2> uvs)
    {
        vertices = new List<Vector3>();
        indices = new List<int>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();

        foreach (var face in geometrySet.GetReferences<IFCFace>("CfsFaces"))
        {
            foreach (var bound in face.GetReferences<IFCFaceBound>("Bounds"))
            {
                var loop = bound.GetReference<IFCLoop>("Bound");
                if (!loop.Is<IFCPolyLoop>(true)) continue;
                List<Vector3> vertsFromLoop = new List<Vector3>();
                foreach (var point in loop.GetReferences<IFCCartesianPoint>("Polygon"))
                {
                    var coords = point.GetProperty("Coordinates").AsList();
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
