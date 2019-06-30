using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class Helpers
{
    // Start is called before the first frame update
    public static int[][] ToJaggedIntArray(this float[,] source)
    {
        var width = source.GetLength(0);
        var height = source.GetLength(1);
        var jaggedArray = new int[height][];

        for (int y = 0; y < height; y++)
        {
            jaggedArray[y] = new int[width];
            for (int x = 0; x < width; x++)
            {
                jaggedArray[y][x] = (int)(source[x, y] * 100);
            }
        }

        return jaggedArray;
    }

    public static void Print(this int[][] map)
    {
        
        for (int i = 0; i < map.Length; i++)
        {
            var sb = new StringBuilder();
            for (int j = 0; j < map[i].Length; j++)
            {
                sb.Append(map[i][j]);
                if(j < map[i].Length-1) sb.Append(",");
            }
            Debug.Log(sb.ToString());
        }
    }
}
