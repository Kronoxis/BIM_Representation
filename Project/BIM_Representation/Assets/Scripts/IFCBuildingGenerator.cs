using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class IFCBuildingGenerator : MonoBehaviour
{
    public List<string> Files = new List<string>();
    public uint BatchSize = 1000;
    public Material DefaultMaterial;
    public Text ParseProgress;

    private List<IFCDataContainer> _dataContainers = new List<IFCDataContainer>();

    // Use this for initialization
    void Start()
    {
        foreach (var file in Files)
        {
            _dataContainers.Add(new IFCDataContainer(file, (uint)((float)BatchSize / Files.Count)));
        }

        CoroutineManager.Instance().CreateCoroutine(UpdateParseProgress(_dataContainers));

        foreach (var container in _dataContainers)
        {
            CoroutineManager.Instance().CreateCoroutine(CreateGeometry(container));
        }
    }

    private IEnumerator UpdateParseProgress(List<IFCDataContainer> containers)
    {
        while (containers.Where(c => !c.IsParsed()).ToList().Count > 0)
        {
            var totalTime = Time.realtimeSinceStartup;
            ParseProgress.text = "Elapsed Time: " + totalTime.ToString("0") + " seconds.\n";
            var totalLines = 0;
            for (int i = 0; i < containers.Count; ++i)
            {
                var container = containers[i];
                FileInfo fi = new FileInfo(Files[i]);
                if (container.IsParsed())
                {
                    ParseProgress.text +=
                        "[" + fi.Name + "]: Completed.\n";
                }
                else
                {
                    ParseProgress.text +=
                        "[" + fi.Name + "]: Parsed " + container.GetParsedLineCount() + " lines...\n";
                }
                totalLines = container.GetParsedLineCount();
            }
            ParseProgress.text += "Average speed: " + (totalLines / totalTime).ToString("0") + " lines/second.\n";
            yield return 0;
        }
        ParseProgress.enabled = false;
    }

    private IEnumerator CreateGeometry(IFCDataContainer container)
    {
        while (!container.IsParsed()) yield return 0;
        var geometrySets = container.GetEntities<IIFCConnectedFaceSet>(true);
        CreateMeshes(geometrySets, container);
    }

    private void CreateMeshes(List<IIFCConnectedFaceSet> geometrySets, IFCDataContainer container)
    {
        foreach (var geometrySet in geometrySets)
            CreateMesh(geometrySet, container);
    }

    private void CreateMesh(IIFCConnectedFaceSet geometrySet, IFCDataContainer container)
    {
        var go = new GameObject();
        go.name = geometrySet.Id.ToString();
        var mf = go.AddComponent<MeshFilter>();
        List<Vector3> vertices;
        List<int> indices;
        List<Vector3> normals;
        List<Vector2> uvs;
        geometrySet.GetMeshFilterBuffers(container, out vertices, out indices, out normals, out uvs);
        mf.mesh.vertices = vertices.ToArray();
        mf.mesh.triangles = indices.ToArray();
        mf.mesh.normals = normals.ToArray();
        mf.mesh.uv = uvs.ToArray();
        var mr = go.AddComponent<MeshRenderer>();
        mr.material = DefaultMaterial;
        //go.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
        go.transform.Rotate(new Vector3(-90, 0, 0));
    }
}
