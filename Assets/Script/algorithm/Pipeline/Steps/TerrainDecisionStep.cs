using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 地形決定 - 還原舊版 TerrainThreshold 邏輯
/// </summary>
[System.Serializable]
public class TerrainDecisionStep : IMapPipelineStep
{
    [System.Serializable]
    public struct TerrainThreshold
    {
        [Tooltip("噪聲值必須低於此閾值")]
        public float threshold;
        public TerrainType terrain;
    }
    
    [Header("Thresholds (按 threshold 從小到大排序)")]
    public List<TerrainThreshold> thresholds = new List<TerrainThreshold>
    {
        new TerrainThreshold { threshold = 0.30f, terrain = TerrainType.Water },
        new TerrainThreshold { threshold = 0.75f, terrain = TerrainType.Grass },
        new TerrainThreshold { threshold = 1.00f, terrain = TerrainType.Rock }
    };
    
    [SerializeField] private string heightLayer = HeightMapStep.LAYER_NAME;
    [SerializeField] private bool enabled = true;
    
    public string StepName => "Terrain Decision";
    public PipelinePhase Phase => PipelinePhase.TerrainDecision;
    public int Priority => 0;
    public bool Enabled { get => enabled; set => enabled = value; }
    
    public void Execute(MapPipelineContext ctx)
    {
        for (int x = 0; x < ctx.Width; x++)
        {
            for (int y = 0; y < ctx.Height; y++)
            {
                float height = ctx.GetValue(heightLayer, x, y);
                ctx.TerrainGrid[x, y] = GetTerrainFromNoise(height);
            }
        }
        
        ctx.SyncToOutputMap();
    }
    
    private TerrainType GetTerrainFromNoise(float noiseValue)
    {
        foreach (var entry in thresholds)
        {
            if (noiseValue <= entry.threshold)
                return entry.terrain;
        }
        return thresholds[thresholds.Count - 1].terrain;
    }
}