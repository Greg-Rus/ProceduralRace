using System;
using System.Linq;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public DrawMode DrawMode;
    public int Height;
    public int Width;
    public float Scale;
    public MapDisplay Display;
    public int Octaves;
    [Range(0,1)] public float Persistence;
    public float Lacunarity;
    public int Seed;
    public Vector2 CustomOffset;

    public TerrainType[] Regions;
    public MeshDeformationType MeshDeformationType;

    public MeshDeformationData BaseMeshDeformationData;
    public MeshDeformationData ValleyMeshDeformationData;
    public MeshDeformationData PassMeshDeformationData;
    

    public bool AutoUpdate;
    // Start is called before the first frame update

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMap()
    {
        var map = NoiseGenerator.GenerateNoiseMap(Width, Height, Seed, Scale, Octaves, Persistence, Lacunarity, CustomOffset);
        map = DeformNoiseMap(map);
        var colorMap = new Color[Width * Height];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var noiseValue = map[x, y];
                var color = Regions.First(region => region.Height >= noiseValue).Color;
                colorMap[y * Width + x] = color;
            }
        }

        switch (DrawMode)
        {
            case DrawMode.NoiseMap:
                Display.DrawTexture(TextureGenerator.TextureFromHeightMap(map, Width, Height));
                break;
            case DrawMode.ColorMap:
                Display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, Width, Height));
                break;
            case DrawMode.Mesh:
                Display.DrawMesh(MeshGenerator.GenerateTerrainMesh(DeformNoiseMap(map), GetCurrentMeshDeformationData()), TextureGenerator.TextureFromColorMap(colorMap, Width, Height));
                break;
            case DrawMode.MeshWithRoad:
                Display.DrawMeshAndRoad(MeshGenerator.GenerateTerrainMesh(DeformNoiseMap(map), GetCurrentMeshDeformationData()), TextureGenerator.TextureFromColorMap(colorMap, Width, Height));

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private float[,] DeformNoiseMap(float[,] map)
    {
        switch (MeshDeformationType)
        {
            case MeshDeformationType.Base:
                return map;
            case MeshDeformationType.Valley:
                return ApplyMeshDeformation(map,ValleyMeshDeformationData);
            case MeshDeformationType.Pass:
                return ApplyMeshDeformation(map, PassMeshDeformationData);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private MeshDeformationData GetCurrentMeshDeformationData()
    {
        switch (MeshDeformationType)
        {
            case MeshDeformationType.Base:
                return BaseMeshDeformationData;
            case MeshDeformationType.Valley:
                return ValleyMeshDeformationData;
            case MeshDeformationType.Pass:
                return PassMeshDeformationData;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private float[,] ApplyMeshDeformation(float[,] originalMap, MeshDeformationData meshDeformationData)
    {
        var width = originalMap.GetLength(0);
        var height = originalMap.GetLength(1);

        var deformedMap = new float[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var averageDeformation = Mathf.Max(meshDeformationData.DeformationCurveX.Evaluate(x / (float) width),
                                          meshDeformationData.DeformationCurveY.Evaluate(y / (float) height));

                deformedMap[x, y] = Mathf.Clamp01(originalMap[x, y] * averageDeformation);
            }
        }

        return deformedMap;
    }


    void OnValidate()
    {
        if (Width < 1) Width = 1;
        if (Height < 1) Height = 1;
        if (Lacunarity < 1) Lacunarity = 1;
        if (Octaves < 0) Octaves = 0;
    }

}

[Serializable]
public struct TerrainType
{
    public string Name;
    public float Height;
    public Color Color;
}

[Serializable]
public class MeshDeformationData
{
    public string Name;
    public float MeshHeightModifier;
    public AnimationCurve DeformationCurveX;
    public AnimationCurve DeformationCurveY;
}

public enum MeshDeformationType
{
    Base = 0,
    Valley,
    Pass
}

