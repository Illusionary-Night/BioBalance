using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class AStarPerformanceTest : MonoBehaviour
{
    [SerializeField] private int mapSize = 100;
    [SerializeField] private float obstacleRatio = 0.3f;
    [SerializeField] private int testIterations = 100;
    
    private bool[,] testMap;
    
    void Start()
    {
        GenerateTestMap();
        RunPerformanceTest();
    }
    
    private void GenerateTestMap()
    {
        testMap = new bool[mapSize, mapSize];
        
        // 生成隨機障礙物
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                testMap[x, y] = Random.value > obstacleRatio;
            }
        }
        
        // 確保起點和終點可通行
        testMap[0, 0] = true;
        testMap[mapSize - 1, mapSize - 1] = true;
    }
    
    private bool IsWalkable(Vector2Int position)
    {
        if (position.x < 0 || position.x >= mapSize || 
            position.y < 0 || position.y >= mapSize)
            return false;
            
        return testMap[position.x, position.y];
    }
    
    private void RunPerformanceTest()
    {
        var stopwatch = new Stopwatch();
        var start = new Vector2Int(0, 0);
        var goal = new Vector2Int(mapSize - 1, mapSize - 1);
        
        // 暖身運行
        for (int i = 0; i < 10; i++)
        {
            AStar.FindPath(start, goal, IsWalkable);
        }
        
        // 正式測試
        stopwatch.Start();
        
        int successfulPaths = 0;
        long totalPathLength = 0;
        
        for (int i = 0; i < testIterations; i++)
        {
            var path = AStar.FindPath(start, goal, IsWalkable);
            if (path != null)
            {
                successfulPaths++;
                totalPathLength += path.Count;
            }
        }
        
        stopwatch.Stop();
        
        // 輸出結果
        UnityEngine.Debug.Log($"A* 效能測試結果:");
        UnityEngine.Debug.Log($"地圖大小: {mapSize}x{mapSize}");
        UnityEngine.Debug.Log($"障礙物比例: {obstacleRatio:P}");
        UnityEngine.Debug.Log($"測試次數: {testIterations}");
        UnityEngine.Debug.Log($"成功找到路徑: {successfulPaths}/{testIterations}");
        UnityEngine.Debug.Log($"平均路徑長度: {(successfulPaths > 0 ? (float)totalPathLength / successfulPaths : 0):F1}");
        UnityEngine.Debug.Log($"總執行時間: {stopwatch.ElapsedMilliseconds} ms");
        UnityEngine.Debug.Log($"平均每次執行時間: {(float)stopwatch.ElapsedMilliseconds / testIterations:F2} ms");
        UnityEngine.Debug.Log($"每秒可執行次數: {testIterations * 1000f / stopwatch.ElapsedMilliseconds:F0}");
    }
}