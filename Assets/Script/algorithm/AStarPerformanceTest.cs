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
        
        // �ͦ��H����ê��
        for (int x = 0; x < mapSize; x++)
        {
            for (int y = 0; y < mapSize; y++)
            {
                testMap[x, y] = Random.value > obstacleRatio;
            }
        }
        
        // �T�O�_�I�M���I�i�q��
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
        
        // �x���B��
        for (int i = 0; i < 10; i++)
        {
            AStar.FindPath(start, goal, IsWalkable);
        }
        
        // ��������
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
        
        // ��X���G
        UnityEngine.Debug.Log($"A* �į���յ��G:");
        UnityEngine.Debug.Log($"�a�Ϥj�p: {mapSize}x{mapSize}");
        UnityEngine.Debug.Log($"��ê�����: {obstacleRatio:P}");
        UnityEngine.Debug.Log($"���զ���: {testIterations}");
        UnityEngine.Debug.Log($"���\�����|: {successfulPaths}/{testIterations}");
        UnityEngine.Debug.Log($"�������|����: {(successfulPaths > 0 ? (float)totalPathLength / successfulPaths : 0):F1}");
        UnityEngine.Debug.Log($"�`����ɶ�: {stopwatch.ElapsedMilliseconds} ms");
        UnityEngine.Debug.Log($"�����C������ɶ�: {(float)stopwatch.ElapsedMilliseconds / testIterations:F2} ms");
        UnityEngine.Debug.Log($"�C��i���榸��: {testIterations * 1000f / stopwatch.ElapsedMilliseconds:F0}");
    }
}