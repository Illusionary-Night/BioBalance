using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class MapVisualizer : MonoBehaviour
{
    [System.Serializable]
    public struct VisualLayer
    {
        public TerrainType terrainType; // 對應的地形
        public Tilemap targetTilemap;   // 對應的 Tilemap
        public int sortingOrder;     // 繪製順序
        public bool useDualGrid;    // 是否使用 Dual-Grid 偏移
    }

    [Header("Debug Layer")]
    public Tilemap debugTilemap;

    [Header("Dual-Grid Layers")]
    [Tooltip("請由底層到高層排列：Element 0 是基底(水)，Element 1 是草...")]
    public List<VisualLayer> visualLayers;

    [Header("Dual-Grid Data")]
    public List<DualGridTileData> dualGridDataList;

    [Header("Simple Tile Data")]
    public List<SimpleTileData> simpleTileDataList;

    private Dictionary<TerrainType, TileBase> debugTileLookup;

    // --- Debug 繪製相關 ---
    public void InitializeDebugResources()
    {
        if (debugTileLookup != null && debugTileLookup.Count > 0) return;
        debugTileLookup = new Dictionary<TerrainType, TileBase>();
        LoadAndCheck(TerrainType.Grass, "Tiles/def_grass_0");
        LoadAndCheck(TerrainType.Water, "Tiles/def_water_0");
        LoadAndCheck(TerrainType.Rock, "Tiles/def_rock_0");
    }
    private void LoadAndCheck(TerrainType type, string path)
    {
        TileBase tile = Resources.Load<TileBase>(path);
        if (tile != null) debugTileLookup.Add(type, tile);
    }
    public void RenderDebugMap(TerrainMap mapData, int width, int height)
    {
        if (debugTilemap == null) return;
        InitializeDebugResources();
        debugTilemap.ClearAllTiles();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TerrainType type = mapData.GetTerrain(new Vector2Int(x, y));
                if (debugTileLookup.TryGetValue(type, out TileBase tile))
                {
                    debugTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
        Debug.Log("MapVisualizer: Debug 層繪製完畢。");
    }
    public void ClearAll()
    {
        if (debugTilemap != null) debugTilemap.ClearAllTiles();
        if (visualLayers != null)
        {
            foreach (var layer in visualLayers) if (layer.targetTilemap != null) layer.targetTilemap.ClearAllTiles();
        }
    }

    // --- 帶有優先級判斷的繪製 ---
    public void RenderDualGridMap(TerrainMap mapData, int width, int height)
    {
        if (visualLayers == null || visualLayers.Count == 0) return;

        // 建立優先級查找表 & 自動設定 Sorting Order
        // Element 0 (水) = Priority 0
        // Element 1 (草) = Priority 1
        Dictionary<TerrainType, int> priorityMap = new Dictionary<TerrainType, int>();
        for (int i = 0; i < visualLayers.Count; i++)
        {
            VisualLayer layer = visualLayers[i];

            // 記錄優先級 (以列表順序為準)
            if (!priorityMap.ContainsKey(layer.terrainType))
            {
                priorityMap.Add(layer.terrainType, i);
            }

            if (layer.targetTilemap != null)
            {
                // 自動設定 Tilemap 的 Sorting Order
                TilemapRenderer renderer = layer.targetTilemap.GetComponent<TilemapRenderer>();
                if (renderer != null)
                {
                    renderer.sortingOrder = layer.sortingOrder;
                }

                // 清空舊畫面
                layer.targetTilemap.ClearAllTiles();
            }
        }

        // 準備圖資
        Dictionary<TerrainType, DualGridTileData> dualDataLookup = new Dictionary<TerrainType, DualGridTileData>();
        foreach (var data in dualGridDataList)
        {
            if (!dualDataLookup.ContainsKey(data.terrainType)) dualDataLookup.Add(data.terrainType, data);
        }

        Dictionary<TerrainType, TileBase> simpleDataLookup = new Dictionary<TerrainType, TileBase>();
        if (simpleTileDataList != null)
        {
            foreach (var data in simpleTileDataList)
            {
                if (!simpleDataLookup.ContainsKey(data.terrainType)) simpleDataLookup.Add(data.terrainType, data.tile);
            }
        }

        // 逐層繪製
        for (int layerIndex = 0; layerIndex < visualLayers.Count; layerIndex++)
        {
            VisualLayer currentLayer = visualLayers[layerIndex];
            if (currentLayer.targetTilemap == null) continue;

            if (currentLayer.useDualGrid)
            {
                if (!dualDataLookup.TryGetValue(currentLayer.terrainType, out DualGridTileData tileData)) continue;

                for (int x = 0; x < width - 1; x++)
                {
                    for (int y = 0; y < height - 1; y++)
                    {
                        // 判斷四個角是否視為 "實心"
                        // 傳入 layerIndex (當前層級) 與 priorityMap (所有地形的地位)
                        bool bl = IsSolid(mapData.GetTerrain(new Vector2Int(x, y)), layerIndex, priorityMap);
                        bool br = IsSolid(mapData.GetTerrain(new Vector2Int(x + 1, y)), layerIndex, priorityMap);
                        bool tl = IsSolid(mapData.GetTerrain(new Vector2Int(x, y + 1)), layerIndex, priorityMap);
                        bool tr = IsSolid(mapData.GetTerrain(new Vector2Int(x + 1, y + 1)), layerIndex, priorityMap);

                        int index = 0;
                        if (tl) index += 8;
                        if (tr) index += 4;
                        if (bl) index += 2;
                        if (br) index += 1;

                        if (index > 0 && index < tileData.tiles.Length)
                        {
                            TileBase tile = tileData.tiles[index];
                            if (tile != null)
                            {
                                currentLayer.targetTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                            }
                        }
                    }
                }
            }
            else
            {

                // 使用簡單圖塊繪製
                if (!simpleDataLookup.TryGetValue(currentLayer.terrainType, out TileBase tile)) continue;
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        TerrainType type = mapData.GetTerrain(new Vector2Int(x, y));
                        if (type == currentLayer.terrainType)
                        {
                            currentLayer.targetTilemap.SetTile(new Vector3Int(x, y, 0), tile);
                        }
                    }
                }
            }
        }
        Debug.Log("MapVisualizer: 混合圖層繪製完畢。");
    }

    // --- 判斷邏輯 ---
    private bool IsSolid(TerrainType targetType, int currentLayerIndex, Dictionary<TerrainType, int> priorityMap)
    {
        if (priorityMap.TryGetValue(targetType, out int targetPriority))
        {
            return targetPriority >= currentLayerIndex;
        }

        return false;
    }
}