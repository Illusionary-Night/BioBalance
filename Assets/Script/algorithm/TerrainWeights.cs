using UnityEngine;
using System.Collections.Generic;

// �a�������w�q
public enum TerrainType
{
    Normal = 0,     // ���q�a�ΡA�v�� 1.0
    Grass = 1,      // ��a�A�v�� 1.0
    Sand = 2,       // �F�a�A�v�� 1.5
    Mountain = 3,   // �s�a�A�v�� 2.0
    Swamp = 4,      // �h�A�A�v�� 2.5
    Road = 5,       // �D���A�v�� 0.5
    HighwayRoad = 6,// ���t�D���A�v�� 0.3
    Water = 7,      // ����A���i�q��
    Lava = 8,       // �����A�v�� 10.0�]�M�I�^
    Ice = 9         // �B���A�v�� 1.2
}

// �a���v���u����
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
        { TerrainType.Water, float.MaxValue }, // ���i�q��
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

// �a�Φa�Ͻd�����O
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

    // �]�w�x�ΰϰ쪺�a��
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
