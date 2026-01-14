using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SpeciesStats
{
    public int currentCount;
    public List<float> countHistory = new List<float>();
    public List<float> birthRateHistory = new List<float>();
    public List<float> deathRateHistory = new List<float>();
    public List<float> hungryRatioHistory = new List<float>();
    public bool isFolded = false;
}

public class StatisticsTableEditor : EditorWindow
{
    private Dictionary<int, SpeciesStats> allSpecies = new();
    private float lastUpdateTime = 0f;
    private const float updateInterval = 0.5f; // 每 0.5 秒紀錄一次數據，避免過快

    [MenuItem("Window/BioBalance/Population Stats")]
    public static void ShowWindow()
    {
        StatisticsTableEditor window = GetWindow<StatisticsTableEditor>("生態統計表");
        window.Show();
    }
    private void OnEnable()
    {
        lastUpdateTime = Time.realtimeSinceStartup;
    }
    private void OnGUI()
    {
        EditorGUILayout.LabelField("全族群監控面板", EditorStyles.boldLabel);

        if (allSpecies.Count == 0)
        {
            EditorGUILayout.HelpBox("等待遊戲運行並產生數據...", MessageType.Info);
            // 點擊可以手動觸發一次更新
            if (GUILayout.Button("手動檢查 Manager 資料"))
            {
                UpdateData();
            }
        }

        foreach (var speciesPair in allSpecies)
        {
            var speciesID = speciesPair.Key;
            var stats = speciesPair.Value;

            EditorGUILayout.BeginVertical("box");
            stats.isFolded = EditorGUILayout.Foldout(stats.isFolded, $"物種 ID: {speciesID} (目前數量: {stats.currentCount})", true);

            if (stats.isFolded)
            {
                EditorGUI.indentLevel++;
                DrawMiniGraph("數量趨勢", stats.countHistory, Color.white);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();
        }
    }

    private void DrawMiniGraph(string label, List<float> data, Color color)
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(label, EditorStyles.miniLabel);

        if (data == null || data.Count < 2)
        {
            EditorGUILayout.LabelField("數據收集不足 (至少需2筆)", EditorStyles.centeredGreyMiniLabel);
        }
        else
        {
            AnimationCurve curve = new AnimationCurve();
            for (int i = 0; i < data.Count; i++)
            {
                curve.AddKey(i, data[i]);
            }

            // 動態計算 Y 軸最大值，讓曲線永遠保持在畫面內
            float maxVal = data.Max()*1.2f;
            if (maxVal < 10) maxVal = 10; // 至少顯示 0~10 的範圍

            // Rect(x, y, width, height) 用來定義曲線在 CurveField 裡的顯示範圍
            EditorGUILayout.CurveField(curve, color, new Rect(0, 0, data.Count, maxVal), GUILayout.Height(200));
        }
        EditorGUILayout.EndVertical();
    }

    private void OnInspectorUpdate()
    {
        if (!Application.isPlaying || !Manager.Instance) return;

        // 定時採樣：不要每幀紀錄，這很重要！
        if (Time.realtimeSinceStartup - lastUpdateTime > updateInterval)
        {
            UpdateData();
            lastUpdateTime = Time.realtimeSinceStartup;
            Repaint();
        }
    }

    private void UpdateData()
    {
        foreach (var species in Manager.Instance.Species)
        {
            int id = species.attributes.species_ID;

            if (!allSpecies.ContainsKey(id))
            {
                allSpecies.Add(id, new SpeciesStats());
            }

            var stats = allSpecies[id];
            stats.currentCount = species.creatures.Count;

            // 紀錄歷史，並限制紀錄上限（例如保留最近 200 筆資料）
            stats.countHistory.Add(stats.currentCount);
            if (stats.countHistory.Count > 200) stats.countHistory.RemoveAt(0);
        }
    }
}