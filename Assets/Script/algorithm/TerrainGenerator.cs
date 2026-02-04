using UnityEngine;
using System.Collections.Generic;

public class TerrainGenerator : MonoBehaviour
{
    public static TerrainGenerator Instance { get; private set; }

    [Header("Map Dimensions")]
    public int mapWidth = 100;
    public int mapHeight = 100;

    [Header("Noise Settings")]
    [Tooltip("噪聲尺度。值越大，地形特徵越大越平滑。")]
    public float noiseScale = 20f;
    [Tooltip("疊加層數。越多層細節越豐富。")]
    public int octaves = 4;
    [Tooltip("持續性 (0-1)，影響細節的強度。")]
    public float persistence = 0.5f;
    [Tooltip("頻率變化率。")]
    public float lacunarity = 2.0f;

    [Header("Map Seed Control")]
    [Tooltip("輸入整數，即可重現同一張地圖。")]
    public int mapSeed = 1337; // 預設值
    [HideInInspector] 
    public Vector2 noiseOffset;

    [Tooltip("定義噪聲值如何映射到地形類型 (請按值從小到大排序)")]
    [Header("Terrain Thresholds")]
    public List<TerrainThreshold> terrainThresholds;

    // 純數據
    private TerrainMap definitionLayerMap;

    [System.Serializable]
    public struct TerrainThreshold
    {
        [Tooltip("噪聲值必須 *低於* 此閾值")]
        public float threshold;
        public TerrainType terrain;
    }

    //void Awake()
    //{
    //    if (Instance != null && Instance != this) Destroy(this.gameObject);
    //    else Instance = this;

    //    definitionLayerMap = new TerrainMap(TerrainType.Water);
    //    GenerateMapData();
    //}

    public TerrainMap GetDefinitionMap()
    {
        return definitionLayerMap;
    }

    public void RandomizeOffset()
    {
        // 產生一個新的隨機整數種子
        mapSeed = Random.Range(int.MinValue, int.MaxValue);
        // 使用新種子產生 Offset
        GenerateNoiseOffsetFromSeed(mapSeed);

        Debug.Log($"TerrainGenerator: 新種子已生成 -> {mapSeed} (Offset: {noiseOffset})");
    }

    public void GenerateMapData()
    {
        GenerateNoiseOffsetFromSeed(mapSeed);
        if (definitionLayerMap == null) definitionLayerMap = new TerrainMap(TerrainType.Water);
        if (Instance == null) Instance = this; // 確保編輯模式下 Instance 存在

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float noiseValue = GetPerlinNoiseValue(x, y);
                TerrainType type = GetTerrainTypeFromNoise(noiseValue);
                definitionLayerMap.SetTerrain(new Vector2Int(x, y), type);
            }
        }
        Debug.Log("TerrainGenerator: 地圖定義層數據計算完畢。");
    }

    public void ClearData()
    {
        // 重新初始化為一個新的、空的地圖，或者設為 null
        definitionLayerMap = new TerrainMap(TerrainType.Water);
        Debug.Log("TerrainGenerator: 數據已重置。");
    }
    // 柏林噪聲計算邏輯

    private void GenerateNoiseOffsetFromSeed(int seed)
    {
        // 使用整數種子初始化亂數生成器
        Random.InitState(seed);

        // 接下來產生的兩個亂數會永遠一樣
        noiseOffset = new Vector2(
            Random.Range(-10000f, 10000f),
            Random.Range(-10000f, 10000f)
        );
    }

    private float GetPerlinNoiseValue(int x, int y)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseValue = 0;
        float maxPossibleHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x / noiseScale) * frequency + noiseOffset.x * frequency;
            float sampleY = (y / noiseScale) * frequency + noiseOffset.y * frequency;
            float perlin = Mathf.PerlinNoise(sampleX, sampleY);
            noiseValue += perlin * amplitude;
            maxPossibleHeight += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        if (maxPossibleHeight > 0) return Mathf.Clamp01(noiseValue / maxPossibleHeight);
        return Mathf.Clamp01(noiseValue);
    }

    private TerrainType GetTerrainTypeFromNoise(float noiseValue)
    {
        foreach (var entry in terrainThresholds)
        {
            if (noiseValue <= entry.threshold) return entry.terrain;
        }
        return terrainThresholds[terrainThresholds.Count - 1].terrain;
    }
}