using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetMesher))]
public class PlanetMesherEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlanetMesher planetScript = (PlanetMesher)target;
        if (GUILayout.Button("Build Object"))
        {
            planetScript.CreatePlanetMesh();
        }
    }

    public void OnValidate()
    {
        PlanetMesher planetScript = (PlanetMesher)target;
        planetScript.CreatePlanetMesh();
    }
}
