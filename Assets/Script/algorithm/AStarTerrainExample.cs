using UnityEngine;
using System.Collections.Generic;

public class AStarTerrainExample : MonoBehaviour
{
    [Header("路徑尋找測試")]
    public Vector2Int startPosition = new Vector2Int(0, 0);
    public Vector2Int goalPosition = new Vector2Int(10, 10);
    
    [Header("地形設定")]
    public bool useTerrainWeights = true;
    
    private TerrainMap terrainMap;
    private List<Vector2Int> currentPath;

    void Start()
    {
        SetupTerrainMap();
        FindPathExample();
    }

    void SetupTerrainMap()
    {
        terrainMap = new TerrainMap(TerrainType.Grass);
        
        // 設定一些範例地形
        // 沙地區域
        terrainMap.SetTerrainArea(new Vector2Int(3, 3), new Vector2Int(6, 6), TerrainType.Sand);
        
        // 山地障礙
        //terrainMap.SetTerrainArea(new Vector2Int(7, 2), new Vector2Int(8, 8), TerrainType.Mountain);
        
        // 道路（快速通道）
        //terrainMap.SetTerrainArea(new Vector2Int(0, 5), new Vector2Int(12, 5), TerrainType.Road);
        
        // 沼澤區域（慢速）
        terrainMap.SetTerrainArea(new Vector2Int(2, 8), new Vector2Int(5, 10), TerrainType.Swamp);
        
        // 水域（不可通行）
        terrainMap.SetTerrainArea(new Vector2Int(9, 9), new Vector2Int(11, 11), TerrainType.Water);
    }

    void FindPathExample()
    {
        Debug.Log("=== A* 路徑尋找測試 ===");

        // 測試不使用地形權重的路徑
        var pathWithoutTerrain = AStar.FindPath(startPosition, goalPosition);
        Debug.Log($"無地形權重路徑長度: {pathWithoutTerrain?.Count ?? 0}");
        if (pathWithoutTerrain != null)
        {
            Debug.Log($"無地形權重路徑: {string.Join(" -> ", pathWithoutTerrain)}");
        }

        if (useTerrainWeights)
        {
            // 測試使用地形權重的路徑
            var pathWithTerrain = AStar.FindPath(startPosition, goalPosition, terrainMap.GetTerrainWeight);
            
            Debug.Log($"有地形權重路徑長度: {pathWithTerrain?.Count ?? 0}");
            if (pathWithTerrain != null)
            {
                Debug.Log($"有地形權重路徑: {string.Join(" -> ", pathWithTerrain)}");
                
                // 計算路徑總成本
                float totalCost = CalculatePathCost(pathWithTerrain);
                Debug.Log($"路徑總成本: {totalCost:F2}");
                
                currentPath = pathWithTerrain;
            }
        }
    }

    bool IsBasicWalkable(Vector2Int position)
    {
        // 基本的可行性檢查（不考慮地形）
        // 這裡可以加入邊界檢查等基本限制
        return position.x >= -5 && position.x <= 15 && 
               position.y >= -5 && position.y <= 15;
    }

    float CalculatePathCost(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return 0f;

        float totalCost = 0f;
        var currentPos = startPosition;

        foreach (var nextPos in path)
        {
            var direction = nextPos - currentPos;
            
            // 計算基礎移動成本
            float baseCost = (direction.x != 0 && direction.y != 0) ? 1.4142136f : 1f;
            
            // 應用地形權重
            float terrainWeight = terrainMap.GetTerrainWeight(nextPos);
            totalCost += baseCost * terrainWeight;
            
            currentPos = nextPos;
        }

        return totalCost;
    }

    // 在場景視圖中繪製地形和路徑（僅在編輯器中）
    void OnDrawGizmos()
    {
        if (terrainMap == null) return;

        // 繪製地形
        for (int x = -2; x <= 15; x++)
        {
            for (int y = -2; y <= 15; y++)
            {
                var pos = new Vector2Int(x, y);
                var terrain = terrainMap.GetTerrain(pos);
                
                Color terrainColor = GetTerrainColor(terrain);
                Gizmos.color = terrainColor;
                Gizmos.DrawCube(new Vector3(x, y, 0), Vector3.one * 0.8f);
            }
        }

        // 繪製起點和終點
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(startPosition.x, startPosition.y, 0), 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(goalPosition.x, goalPosition.y, 0), 0.5f);

        // 繪製路徑
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.yellow;
            var current = new Vector3(startPosition.x, startPosition.y, 0);
            
            foreach (var point in currentPath)
            {
                var next = new Vector3(point.x, point.y, 0);
                Gizmos.DrawLine(current, next);
                Gizmos.DrawSphere(next, 0.2f);
                current = next;
            }
        }
    }

    Color GetTerrainColor(TerrainType terrain)
    {
        return terrain switch
        {
            //TerrainType.Normal => Color.white,
            TerrainType.Grass => Color.green,
            TerrainType.Sand => Color.yellow,
            //TerrainType.Mountain => Color.gray,
            TerrainType.Swamp => new Color(0.5f, 0.3f, 0.1f), // 棕色
            //TerrainType.Road => new Color(0.3f, 0.3f, 0.3f), // 深灰
            //TerrainType.HighwayRoad => Color.black,
            TerrainType.Water => Color.blue,
            TerrainType.Lava => Color.red,
            TerrainType.Ice => Color.cyan,
            _ => Color.white
        };
    }

    [ContextMenu("重新計算路徑")]
    public void RecalculatePath()
    {
        FindPathExample();
    }
}

// 效能比較工具
public static class AStarPerformanceComparison
{
    public static void ComparePathfindingMethods(Vector2Int start, Vector2Int goal, TerrainMap terrainMap, int iterations = 1000)
    {
        Debug.Log("=== A* 效能比較測試 ===");

        // 測試不使用地形權重
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            AStar.FindPath(start, goal);
        }
        stopwatch.Stop();
        Debug.Log($"無地形權重 {iterations} 次測試: {stopwatch.ElapsedMilliseconds}ms (平均: {stopwatch.ElapsedMilliseconds / (float)iterations:F3}ms)");

        // 測試使用地形權重
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            AStar.FindPath(start, goal, terrainMap.GetTerrainWeight);
        }
        stopwatch.Stop();
        Debug.Log($"有地形權重 {iterations} 次測試: {stopwatch.ElapsedMilliseconds}ms (平均: {stopwatch.ElapsedMilliseconds / (float)iterations:F3}ms)");
    }
}