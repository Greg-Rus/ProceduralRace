using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;

public class MapDisplay : MonoBehaviour
{
    public Renderer Renderer;

    public MeshFilter MeshFilter;

    public MeshRenderer MeshRenderer;

    public RoadMeshCreator RoadMeshCreator;

    public int RoadSmoothRange;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DrawTexture(Texture2D texture)
    {
        Renderer.sharedMaterial.mainTexture = texture;
        Renderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        MeshFilter.sharedMesh = meshData.CreateMesh();
        MeshRenderer.sharedMaterial.mainTexture = texture;
    }

    public void DrawMeshAndRoad(MeshData meshData, Texture2D texture)
    {
        SmoothOutRoadTerrain(meshData);

        DrawMesh(meshData, texture);
        var meshMiddle = meshData.MeshWidth / 2;
        var middleVertices = new Vector3[meshData.MeshHeight];
        for (int i = 0; i < meshData.MeshHeight; i++)
        {
            var index = meshMiddle + meshData.MeshWidth * i;
            middleVertices[i] = meshData.Vertices[index];
        }
        BezierPath bezierPath = new BezierPath(middleVertices);
        VertexPath vertexPath = new VertexPath(bezierPath,10,0.5f);
        RoadMeshCreator.GenerateRoad(vertexPath);
    }

    private void SmoothOutRoadTerrain(MeshData meshData)
    {
        var meshMiddle = meshData.MeshWidth / 2;
        for (int i = 0; i < meshData.MeshHeight; i++)
        {
            var index = meshMiddle + meshData.MeshWidth * i;
            for (int x = index - RoadSmoothRange; x < index + RoadSmoothRange; x++)
            {
                meshData.Vertices[x] = new Vector3(meshData.Vertices[x].x, meshData.Vertices[index].y, meshData.Vertices[x].z);
            }

            //var averageIndex = RoadSmoothRange + 1;
            //meshData.Vertices[index - averageIndex] = 
            //    new Vector3(meshData.Vertices[index - averageIndex].x, (meshData.Vertices[index].y + meshData.Vertices[index - averageIndex].y) / 2, meshData.Vertices[index- averageIndex].z);

            //meshData.Vertices[index + averageIndex] = 
            //    new Vector3(meshData.Vertices[index + averageIndex].x, (meshData.Vertices[index].y + meshData.Vertices[index + averageIndex].y) / 2, meshData.Vertices[index+ averageIndex].z);
        }
    }

}
