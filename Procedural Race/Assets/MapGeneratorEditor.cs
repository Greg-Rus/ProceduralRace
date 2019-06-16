using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        MapGenerator generator = (MapGenerator)target;
        if (DrawDefaultInspector() && generator.AutoUpdate)
        {
            generator.GenerateMap();
        }
        
        if (GUILayout.Button("Generate Noise"))
        {
            generator.GenerateMap();
        }
    }
}
