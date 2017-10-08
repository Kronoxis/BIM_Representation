using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//[ExecuteInEditMode]
public class MakeDoubleFaced : MonoBehaviour
{
    public Transform Parent;

    private void Update()
    {
        var meshFilters = GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length > 0)
        {
            CreateDoubleSidedMeshes(meshFilters);
            Debug.Log("Applied Double Faces to " + meshFilters.Length + " objects");
            MoveToParent();
        }
    }

    private void CreateDoubleSidedMeshes(MeshFilter[] meshFilters)
    {
        foreach (var meshFilter in meshFilters)
        {
            var mesh = meshFilter.mesh;
            var vertices = mesh.vertices;
            var normals = mesh.normals;
            var uvs = mesh.uv;
            var vertexCount = vertices.Length;
            var newVertices = new Vector3[vertexCount * 2];
            var newNormals = new Vector3[vertexCount * 2];
            var newUvs = new Vector2[vertexCount * 2];

            // Vertex Buffer
            for (var i = 0; i < vertexCount; i++)
            {
                if (vertices.Length > 0)
                {
                    // Duplicate Vertices
                    newVertices[i] = vertices[i];
                    newVertices[i + vertexCount] = vertices[i];
                }
                if (uvs.Length > 0)
                {
                    // Duplicate UVs
                    newUvs[i] = uvs[i];
                    newUvs[i + vertexCount] = uvs[i];
                }
                if (normals.Length > 0)
                {
                    // Copy Original Normals
                    newNormals[i] = normals[i];
                    // Invert second face normals
                    newNormals[i + vertexCount] = -normals[i];
                }
            }

            // Index Buffer
            var indices = mesh.triangles;
            var indexCount = indices.Length;
            var newIndices = new int[indexCount * 2];
            for (var i = 0; i < indexCount; i += 3)
            {
                // copy the original triangle
                if (indices.Length > 0)
                {
                    // Copy the original triangle
                    newIndices[i] = indices[i];
                    newIndices[i + 1] = indices[i + 1];
                    newIndices[i + 2] = indices[i + 2];
                    // Add a new triangle in reversed order
                    newIndices[i + indexCount] = indices[i] + vertexCount;
                    newIndices[i + indexCount + 2] = indices[i + 1] + vertexCount;
                    newIndices[i + indexCount + 1] = indices[i + 2] + vertexCount;
                }
            }
            mesh.vertices = newVertices;
            mesh.triangles = newIndices;
            mesh.normals = newNormals;
            mesh.uv = newUvs;
        }
    }

    private void MoveToParent()
    {
        while (transform.childCount > 0)
        {
            transform.GetChild(0).parent = Parent;
        }
    }
}
