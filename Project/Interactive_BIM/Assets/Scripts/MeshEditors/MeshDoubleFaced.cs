using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class MeshDoubleFaced : MonoBehaviour
{
    public void DoubleFace(MeshFilter meshFilter)
    {
        var mesh = meshFilter.sharedMesh;
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
        meshFilter.gameObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void DoubleObject(GameObject go)
    {
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        var uvs = mesh.uv;
        var vertexCount = vertices.Length;
        var newVertices = new Vector3[vertexCount];
        var newNormals = new Vector3[vertexCount];
        var newUvs = new Vector2[vertexCount];

        // Vertex Buffer
        for (var i = 0; i < vertexCount; i++)
        {
            // Duplicate Vertices
            newVertices = vertices;
            // Duplicate UVs
            newUvs = uvs;
            // Invert face normals
            if (normals.Length > 0)
                newNormals[i] = -normals[i];
        }

        // Index Buffer
        var indices = mesh.triangles;
        var indexCount = indices.Length;
        var newIndices = new int[indexCount];
        for (var i = 0; i < indexCount; i += 3)
        {
            // Add a new triangle in reversed order
            if (indices.Length > 0)
            {
                newIndices[i] = indices[i];
                newIndices[i + 2] = indices[i + 1];
                newIndices[i + 1] = indices[i + 2];
            }
        }

        GameObject backFace = new GameObject(go.name + "_Backface");
        backFace.transform.SetParent(go.transform.parent);
        backFace.transform.SetPositionAndRotation(go.transform.position, go.transform.rotation);
        Mesh bfMesh = new Mesh
        {
            vertices = newVertices,
            triangles = newIndices,
            normals = newNormals,
            uv = newUvs
        };
        bfMesh.name = backFace.name;
        backFace.AddComponent<MeshFilter>().sharedMesh = bfMesh;
        backFace.AddComponent<MeshRenderer>().sharedMaterials = go.GetComponent<MeshRenderer>().sharedMaterials;
        backFace.AddComponent<MeshCollider>().sharedMesh = bfMesh;
    }
}
