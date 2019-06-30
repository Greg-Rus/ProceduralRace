using System;
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

    public int RoadNodeDensityFactor = 1;

    public int PathFindingNodeDensityFactor = 1;
    public GameObject EntryMarker;
    public GameObject ExitMarker;

    

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

    public void DrawMeshAndPlotRoad(MeshData meshData, Texture2D texture, float[,] heightMap)
    {
        var graph = new SquareGrid(heightMap, PathFindingNodeDensityFactor);
        var entryPoint = graph.GetLocationAtCoordinates(graph.width / 2, 0);
        EntryMarker.transform.position = meshData.GetVertexAt(entryPoint.x * PathFindingNodeDensityFactor , entryPoint.y * PathFindingNodeDensityFactor)+
            Vector3.up * 10;
        var exitPoint = graph.GetLocationAtCoordinates(graph.width / 2, graph.height - 1);
        ExitMarker.transform.position = meshData.GetVertexAt(exitPoint.x * PathFindingNodeDensityFactor, exitPoint.y * PathFindingNodeDensityFactor) +
                                        Vector3.up * 10;

        var pathing = new Pathing(graph, entryPoint, exitPoint);
        var path = pathing.WhereTo();

        
        var RoadVertices = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            var vertex = meshData.Vertices[(path[i].x ) + (path[i].y  * meshData.MeshWidth)];
            RoadVertices[i] = vertex;
        }

        

        BezierPath bezierPath = RoadNodeDensityFactor == 1
            ? new BezierPath(RoadVertices)
            : new BezierPath(SmoothRoad(RoadVertices));

        VertexPath vertexPath = new VertexPath(bezierPath);//, 10, 0.5f);

        RoadMeshCreator.GenerateRoad(vertexPath);
        FlattenTerrainAroundRoad(meshData, path, texture, vertexPath);
        DrawMesh(meshData, texture);
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
        }
    }

    private void FlattenTerrainAroundRoad(MeshData meshData, List<Location> path, Texture2D texture, VertexPath vertexPath)
    {
        var verticesToSmooth = new HashSet<Vector2Int>();
        for (int i = 0; i < path.Count-1; i++)
        {
            for (int x = path[i].x - RoadSmoothRange; x < path[i].x + RoadSmoothRange; x++)
            {
                for (int y = path[i].y - RoadSmoothRange; y < path[i].y + RoadSmoothRange; y++)
                {
                    if (x < 0 || x > meshData.MeshWidth-1 || y < 0 || y > meshData.MeshHeight-1) continue;

                    verticesToSmooth.Add(new Vector2Int(x, y));
                    //texture.SetPixel(x,y,Color.grey);
                }
            }
        }

        var pathVertices = new List<Vector3>();

        for (int i = 0; i < meshData.MeshHeight; i++)
        {
            pathVertices.Add(vertexPath.GetPoint(i /(float)meshData.MeshHeight));
        }

        foreach (var vertexPosition in verticesToSmooth)
        {
            var vertex = meshData.GetVertexAt(vertexPosition.x, vertexPosition.y);
            Vector3 closest = pathVertices[0];
            float distanceToClosest = (vertex - closest).sqrMagnitude;
            for (int i = 1; i < pathVertices.Count; i++)
            {
                var distance = (vertex - pathVertices[i]).sqrMagnitude;
                if (distance < distanceToClosest)
                {
                    closest = pathVertices[i];
                    distanceToClosest = distance;
                }
            }
            meshData.SetVertexAt(vertexPosition.x, vertexPosition.y, new Vector3(vertex.x, closest.y, vertex.z));
        }



        texture.Apply();
    }

    private Vector3[] SmoothRoad(Vector3[] roadVertices)
    {
        var smoothedRoad = new Vector3[roadVertices.Length / RoadNodeDensityFactor];
        for (int i = 0; i < smoothedRoad.Length; i++)
        {
            smoothedRoad[i] = roadVertices[i*RoadNodeDensityFactor];
        }
        return smoothedRoad;
    }



}
