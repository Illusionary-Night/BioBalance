using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

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
    public Vector2 noiseOffset; // 用於隨機"種子"

    [Header("Dual-Grid System")]
    [Tooltip("用於繪製 Debug 視覺的 Tilemap")]
    public Tilemap debugDisplayTilemap;

    [Header("Terrain Thresholds")]
    [Tooltip("定義噪聲值如何映射到地形類型 (請按值從小到大排序)")]
    public List<TerrainThreshold> terrainThresholds;

    // 「定義層」(Data Layer) - 儲存所有地圖邏輯
    private TerrainMap definitionLayerMap;

    // 用於快速查找 TerrainType 對應的 TileBase
    private Dictionary<TerrainType, TileBase> tileLookup;


    // 讓 AI 和 A* 尋路系統 獲取地圖數據層

    public TerrainMap GetDefinitionMap()
    {
        return definitionLayerMap;
    }

    // 用於在 Inspector 中設定閾值
    [System.Serializable]
    public struct TerrainThreshold
    {
        [Tooltip("噪聲值必須 *低於* 此閾值")]
        public float threshold;
        // 使用您在 Constant.cs 中定義的 TerrainType
        public TerrainType terrain;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // 如果場景中已經存在一個實例，則銷毀這個重複的
            Destroy(this.gameObject);
        }
        else
        {
            // 否則，將自己設定為這個靜態實例
            Instance = this;
        }
        // 1. 建立一個空的「定義層」
        definitionLayerMap = new TerrainMap(TerrainType.Grass);

        // 2. (新) 在程式碼中指定並載入 Tile 資產
        LoadTilesFromResources();
    }

    void Start()
    {
        // 3. 產生隨機偏移 (種子)
        if (noiseOffset == Vector2.zero)
        {
            noiseOffset = new Vector2(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));
        }

        // 4. 執行生成
        GenerateMap();
    }


    void LoadTilesFromResources()
    {
        tileLookup = new Dictionary<TerrainType, TileBase>();

        // 警告：Resources.Load() 速度較慢，只應在 Awake/Start 中呼叫
        // 假設您的 Tile Assets 放在 "Assets/Resources/Tiles/" 資料夾中

        // *** 這是您要求「在程式碼中指定」的地方 ***
        // 路徑是相對於 "Resources" 資料夾的，且 "不能" 包含副檔名

        tileLookup.Add(TerrainType.Grass, Resources.Load<TileBase>("Tiles/def_grass_0"));
        tileLookup.Add(TerrainType.Water, Resources.Load<TileBase>("Tiles/def_water_0"));

        // 您可以繼續為 Constant.cs 中其他的地形添加 Tile
        // tileLookup.Add(TerrainType.Sand, Resources.Load<TileBase>("Tiles/def_sand_tile"));
        // tileLookup.Add(TerrainType.Rock, Resources.Load<TileBase>("Tiles/def_rock_tile"));
        // tileLookup.Add(TerrainType.Barrier, Resources.Load<TileBase>("Tiles/def_barrier_tile")); 

        // 檢查是否有漏載入
        foreach (var tilePair in tileLookup)
        {
            if (tilePair.Value == null)
            {
                Debug.LogError($"載入 Tile 失敗！找不到路徑：'Resources/Tiles/{tilePair.Key.ToString().ToLower()}_tile'");
            }
        }
    }
    // 產生柏林噪聲並填充「定義層」

    public void GenerateMap()
    {
        // 1. 建立一個空的「定義層」
        definitionLayerMap = new TerrainMap(TerrainType.Grass);

        // 2. 迴圈遍歷所有座標
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // 3. 計算柏林噪聲值
                float noiseValue = GetPerlinNoiseValue(x, y);

                // 4. 將噪聲值映射到 TerrainType
                TerrainType type = GetTerrainTypeFromNoise(noiseValue);
                Vector2Int pos = new Vector2Int(x, y);

                // 5. 儲存數據到「定義層」
                definitionLayerMap.SetTerrain(pos, type);

                // 6. (新增) 繪製 Tile 到「Debug 顯示層」
                if (tileLookup.TryGetValue(type, out TileBase tile))
                {
                    if (tile != null) // 再次檢查，確保 tile 成功載入
                    {
                        // 直接 1:1 繪製，(0,0) 對應 (0,0)
                        debugDisplayTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                    }
                }
            }
        }

        Debug.Log("地圖定義層 (TerrainMap) 生成完畢。");
    }


    // 計算多層柏林噪聲以增加細節
    private float GetPerlinNoiseValue(int x, int y)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseValue = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x / noiseScale) * frequency + noiseOffset.x * frequency;
            float sampleY = (y / noiseScale) * frequency + noiseOffset.y * frequency;

            // Unity 內建的柏林噪聲 (回傳值 0-1)
            float perlin = Mathf.PerlinNoise(sampleX, sampleY);
            noiseValue += perlin * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }
        return noiseValue;
    }

    // 根據噪聲值和您在 Inspector 中設定的閾值來決定地形
    private TerrainType GetTerrainTypeFromNoise(float noiseValue)
    {
        // 從列表頂部 (最低閾值) 開始檢查
        foreach (var entry in terrainThresholds)
        {
            if (noiseValue <= entry.threshold)
            {
                return entry.terrain;
            }
        }
        // 如果大於所有閾值，返回最後一個 (作為最高海拔)
        return terrainThresholds[terrainThresholds.Count - 1].terrain;
    }
}
