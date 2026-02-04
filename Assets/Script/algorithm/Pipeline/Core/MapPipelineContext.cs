using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Pipeline 共享資料容器
/// </summary>
public class MapPipelineContext
{
    public int Width { get; }
    public int Height { get; }
    public int Seed { get; set; }
    public Vector2 NoiseOffset { get; set; }
    
    private readonly Dictionary<string, float[,]> dataLayers = new Dictionary<string, float[,]>();
    
    public TerrainType[,] TerrainGrid { get; set; }
    public TerrainMap OutputMap { get; }
    
    public MapPipelineContext(int width, int height, int seed)
    {
        Width = width;
        Height = height;
        Seed = seed;
        TerrainGrid = new TerrainType[width, height];
        OutputMap = new TerrainMap(TerrainType.Water);
    }
    
    public void CreateLayer(string name)
    {
        if (!dataLayers.ContainsKey(name))
            dataLayers[name] = new float[Width, Height];
    }
    
    public bool HasLayer(string name) => dataLayers.ContainsKey(name);
    
    public float GetValue(string layer, int x, int y)
    {
        if (dataLayers.TryGetValue(layer, out var data))
            return data[x, y];
        return 0f;
    }
    
    public void SetValue(string layer, int x, int y, float value)
    {
        if (dataLayers.TryGetValue(layer, out var data))
            data[x, y] = value;
    }
    
    public void SyncToOutputMap()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                OutputMap.SetTerrain(new Vector2Int(x, y), TerrainGrid[x, y]);
    }
}