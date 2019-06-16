using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colors, int width, int height)
    {
        var coloredTexture = new Texture2D(width,height);
        coloredTexture.filterMode = FilterMode.Point;
        coloredTexture.wrapMode = TextureWrapMode.Clamp;
        coloredTexture.SetPixels(colors);
        coloredTexture.Apply();
        return coloredTexture;
    }

    public static Texture2D TextureFromHeightMap(float[,] map, int width, int height)
    {
        var colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < height; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.white, Color.black, map[x, y]);
            }
        }
        return TextureFromColorMap(colorMap, width, height);
    }
}
