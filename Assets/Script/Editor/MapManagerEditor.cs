using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapManager))]
public class MapManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 1. Draw default variables (Generator, Visualizer slots)
        base.OnInspectorGUI();

        MapManager manager = (MapManager)target;

        GUILayout.Space(20);
        GUILayout.Label("Map Control Center", EditorStyles.boldLabel);

        // --- Roll Seed  ---
        GUI.backgroundColor = new Color(0.6f, 0.8f, 1f); // Light Blue
        if (GUILayout.Button("Roll Seed & Redraw", GUILayout.Height(40)))
        {
            manager.RollAndRedraw();
        }
        GUI.backgroundColor = Color.white; // Restore color

        GUILayout.Space(10);

        // --- 1. Generate Data ---
        if (GUILayout.Button("1. Generate Data Only"))
        {
            manager.GenerateDataOnly();
        }

        // --- 2 & 3. Draw Layers ---
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("2. Draw Debug Layer"))
        {
            manager.DrawDebugLayer();
        }
        if (GUILayout.Button("3. Draw Dual-Grid Layer"))
        {
            manager.DrawDualGridLayer();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        // --- Regenerate (Keep Seed) ---
        GUI.backgroundColor = new Color(0.7f, 1f, 0.7f); // Light Green
        if (GUILayout.Button("Regenerate & Draw (Keep Seed)", GUILayout.Height(30)))
        {
            manager.GenerateAndDrawAll();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);

        // --- Reset ---
        GUI.backgroundColor = new Color(1f, 0.6f, 0.6f); // Light Red
        if (GUILayout.Button("Reset Map", GUILayout.Height(25)))
        {
            manager.ResetMap();
        }
        GUI.backgroundColor = Color.white;
    }
}