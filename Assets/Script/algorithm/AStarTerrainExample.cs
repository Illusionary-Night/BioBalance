using UnityEngine;
using System.Collections.Generic;

public class AStarTerrainExample : MonoBehaviour
{
    [Header("���|�M�����")]
    public Vector2Int startPosition = new Vector2Int(0, 0);
    public Vector2Int goalPosition = new Vector2Int(10, 10);
    
    [Header("�a�γ]�w")]
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
        terrainMap = new TerrainMap(TerrainType.Normal);
        
        // �]�w�@�ǽd�Ҧa��
        // �F�a�ϰ�
        terrainMap.SetTerrainArea(new Vector2Int(3, 3), new Vector2Int(6, 6), TerrainType.Sand);
        
        // �s�a��ê
        terrainMap.SetTerrainArea(new Vector2Int(7, 2), new Vector2Int(8, 8), TerrainType.Mountain);
        
        // �D���]�ֳt�q�D�^
        terrainMap.SetTerrainArea(new Vector2Int(0, 5), new Vector2Int(12, 5), TerrainType.Road);
        
        // �h�A�ϰ�]�C�t�^
        terrainMap.SetTerrainArea(new Vector2Int(2, 8), new Vector2Int(5, 10), TerrainType.Swamp);
        
        // ����]���i�q��^
        terrainMap.SetTerrainArea(new Vector2Int(9, 9), new Vector2Int(11, 11), TerrainType.Water);
    }

    void FindPathExample()
    {
        Debug.Log("=== A* ���|�M����� ===");

        // ���դ��ϥΦa���v�������|
        var pathWithoutTerrain = AStar.FindPath(startPosition, goalPosition, IsBasicWalkable);
        Debug.Log($"�L�a���v�����|����: {pathWithoutTerrain?.Count ?? 0}");
        if (pathWithoutTerrain != null)
        {
            Debug.Log($"�L�a���v�����|: {string.Join(" -> ", pathWithoutTerrain)}");
        }

        if (useTerrainWeights)
        {
            // ���ըϥΦa���v�������|
            var pathWithTerrain = AStar.FindPath(startPosition, goalPosition, 
                terrainMap.IsWalkable, terrainMap.GetTerrainWeight);
            
            Debug.Log($"���a���v�����|����: {pathWithTerrain?.Count ?? 0}");
            if (pathWithTerrain != null)
            {
                Debug.Log($"���a���v�����|: {string.Join(" -> ", pathWithTerrain)}");
                
                // �p����|�`����
                float totalCost = CalculatePathCost(pathWithTerrain);
                Debug.Log($"���|�`����: {totalCost:F2}");
                
                currentPath = pathWithTerrain;
            }
        }
    }

    bool IsBasicWalkable(Vector2Int position)
    {
        // �򥻪��i����ˬd�]���Ҽ{�a�Ρ^
        // �o�̥i�H�[�J����ˬd���򥻭���
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
            
            // �p���¦���ʦ���
            float baseCost = (direction.x != 0 && direction.y != 0) ? 1.4142136f : 1f;
            
            // ���Φa���v��
            float terrainWeight = terrainMap.GetTerrainWeight(nextPos);
            totalCost += baseCost * terrainWeight;
            
            currentPos = nextPos;
        }

        return totalCost;
    }

    // �b�������Ϥ�ø�s�a�ΩM���|�]�Ȧb�s�边���^
    void OnDrawGizmos()
    {
        if (terrainMap == null) return;

        // ø�s�a��
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

        // ø�s�_�I�M���I
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(new Vector3(startPosition.x, startPosition.y, 0), 0.5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(new Vector3(goalPosition.x, goalPosition.y, 0), 0.5f);

        // ø�s���|
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
            TerrainType.Normal => Color.white,
            TerrainType.Grass => Color.green,
            TerrainType.Sand => Color.yellow,
            TerrainType.Mountain => Color.gray,
            TerrainType.Swamp => new Color(0.5f, 0.3f, 0.1f), // �Ħ�
            TerrainType.Road => new Color(0.3f, 0.3f, 0.3f), // �`��
            TerrainType.HighwayRoad => Color.black,
            TerrainType.Water => Color.blue,
            TerrainType.Lava => Color.red,
            TerrainType.Ice => Color.cyan,
            _ => Color.white
        };
    }

    [ContextMenu("���s�p����|")]
    public void RecalculatePath()
    {
        FindPathExample();
    }
}

// �į����u��
public static class AStarPerformanceComparison
{
    public static void ComparePathfindingMethods(Vector2Int start, Vector2Int goal, TerrainMap terrainMap, int iterations = 1000)
    {
        Debug.Log("=== A* �į������� ===");

        // ���դ��ϥΦa���v��
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            AStar.FindPath(start, goal, pos => terrainMap.IsWalkable(pos));
        }
        stopwatch.Stop();
        Debug.Log($"�L�a���v�� {iterations} ������: {stopwatch.ElapsedMilliseconds}ms (����: {stopwatch.ElapsedMilliseconds / (float)iterations:F3}ms)");

        // ���ըϥΦa���v��
        stopwatch.Restart();
        for (int i = 0; i < iterations; i++)
        {
            AStar.FindPath(start, goal, terrainMap.IsWalkable, terrainMap.GetTerrainWeight);
        }
        stopwatch.Stop();
        Debug.Log($"���a���v�� {iterations} ������: {stopwatch.ElapsedMilliseconds}ms (����: {stopwatch.ElapsedMilliseconds / (float)iterations:F3}ms)");
    }
}