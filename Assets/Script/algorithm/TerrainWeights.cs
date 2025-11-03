using UnityEngine;
using System.Collections.Generic;

// 地形權重工具類
public static class TerrainWeights
{
    public static float GetWeight(TerrainType terrainType)
    {
        return DefaultTerrainCosts.TerrainCosts.TryGetValue(terrainType, out float weight) ? weight : 1.0f;
    }

    public static bool IsWalkable(TerrainType terrainType)
    {
        return terrainType != TerrainType.Water && GetWeight(terrainType) < float.MaxValue;
    }
}

// 地形地圖範例類別
public class TerrainMap
{
    private readonly Dictionary<Vector2Int, TerrainType> terrainData = new Dictionary<Vector2Int, TerrainType>();
    private readonly TerrainType defaultTerrain;

    public TerrainMap(TerrainType defaultTerrain = TerrainType.Grass)
    {
        this.defaultTerrain = defaultTerrain;
    }

    public void SetTerrain(Vector2Int position, TerrainType terrainType)
    {
        terrainData[position] = terrainType;
    }

    public TerrainType GetTerrain(Vector2Int position)
    {
        return terrainData.TryGetValue(position, out TerrainType terrain) ? terrain : defaultTerrain;
    }

    public float GetTerrainWeight(Vector2Int position)
    {
        return TerrainWeights.GetWeight(GetTerrain(position));
    }

    public bool IsWalkable(Vector2Int position)
    {
        return TerrainWeights.IsWalkable(GetTerrain(position));
    }

    // 設定矩形區域的地形
    public void SetTerrainArea(Vector2Int min, Vector2Int max, TerrainType terrainType)
    {
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                SetTerrain(new Vector2Int(x, y), terrainType);
            }
        }
    }
}
