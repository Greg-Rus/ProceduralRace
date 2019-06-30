using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, MeshDeformationData deformationData)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        int vertexIndex = 0;
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / -2f;
        MeshData meshData = new MeshData(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                meshData.Vertices[vertexIndex] = new Vector3(topLeftX + x, heightMap[x,y] * deformationData.MeshHeightModifier, topLeftZ + y);
                meshData.Uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);


                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangles(vertexIndex + width, vertexIndex+width+1, vertexIndex);
                    meshData.AddTriangles(vertexIndex + width + 1, vertexIndex+1, vertexIndex);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData
{
    public Vector3[] Vertices;
    public int[] Triangles;
    public Vector2[] Uvs;
    private int _triangleIndex = 0;
    public int MeshWidth;
    public int MeshHeight;


    public MeshData(int meshWidth, int meshHeight)
    {
        Triangles = new int[(meshWidth-1) * (meshHeight-1) * 6];
        Vertices = new Vector3[meshWidth * meshHeight];
        Uvs = new Vector2[meshHeight * meshWidth];
        MeshHeight = meshHeight;
        MeshWidth = meshWidth;
    }

    public void AddTriangles(int a, int b, int c)
    {
        Triangles[_triangleIndex] = a;
        Triangles[_triangleIndex+1] = b;
        Triangles[_triangleIndex+2] = c;
        _triangleIndex += 3;
    }

    public Vector3 GetVertexAt(int x, int y)
    {
        return Vertices[x + y * MeshWidth];
    }

    public void SetVertexAt(int x, int y, Vector3 vertex)
    {
        Vertices[x + y * MeshWidth] = vertex;
    }

    public Mesh CreateMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.triangles = Triangles;
        mesh.uv = Uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

}