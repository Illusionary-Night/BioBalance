using UnityEngine;
using System.Collections.Generic;

// 地形類型定義
public enum TerrainType
{
    Normal = 0,     // 普通地形，權重 1.0
    Grass = 1,      // 草地，權重 1.0
    Sand = 2,       // 沙地，權重 1.5
    Mountain = 3,   // 山地，權重 2.0
    Swamp = 4,      // 沼澤，權重 2.5
    Road = 5,       // 道路，權重 0.5
    HighwayRoad = 6,// 高速道路，權重 0.3
    Water = 7,      // 水域，不可通行
    Lava = 8,       // 熔岩，權重 10.0（危險）
    Ice = 9         // 冰面，權重 1.2
}

// 地形權重工具類
public static class TerrainWeights
{
    private static readonly Dictionary<TerrainType, float> TerrainCosts = new Dictionary<TerrainType, float>
    {
        { TerrainType.Normal, 1.0f },
        { TerrainType.Grass, 1.0f },
        { TerrainType.Sand, 1.5f },
        { TerrainType.Mountain, 2.0f },
        { TerrainType.Swamp, 2.5f },
        { TerrainType.Road, 0.5f },
        { TerrainType.HighwayRoad, 0.3f },
        { TerrainType.Water, float.MaxValue }, // 不可通行
        { TerrainType.Lava, 10.0f },
        { TerrainType.Ice, 1.2f }
    };

    public static float GetWeight(TerrainType terrainType)
    {
        return TerrainCosts.TryGetValue(terrainType, out float weight) ? weight : 1.0f;
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

    public TerrainMap(TerrainType defaultTerrain = TerrainType.Normal)
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
