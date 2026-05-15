using System.Collections.Generic;

public class StatAllocator
{
    // 紀錄玩家在各個節點上投資的點數 (Key: 節點資料, Value: 投入的點數)
    private Dictionary<UpgradeNodeData, int> allocatedPoints;

    // 玩家目前剩餘可用來分配的點數
    public int AvailablePoints { get; private set; }

    // 建構子：初始化分配器，並給予初始可用點數
    public StatAllocator(int startingPoints)
    {
        allocatedPoints = new Dictionary<UpgradeNodeData, int>();
        AvailablePoints = startingPoints;
    }

    /// <summary>
    /// 為指定的節點增加 1 點
    /// </summary>
    public bool AllocatePoint(UpgradeNodeData node)
    {
        int current = GetAllocatedPoints(node);

        // 檢查是否有剩餘點數，且該節點尚未達到上限
        if (AvailablePoints > 0 && current < node.MaxAllocatablePoints)
        {
            allocatedPoints[node] = current + 1;
            AvailablePoints--;
            return true; // 加點成功
        }
        return false; // 加點失敗（點數不足或已達上限）
    }

    /// <summary>
    /// 從指定的節點退回 1 點
    /// </summary>
    public bool RemovePoint(UpgradeNodeData node)
    {
        int current = GetAllocatedPoints(node);

        // 檢查該節點是否至少有 1 點可以退
        if (current > 0)
        {
            allocatedPoints[node] = current - 1;
            AvailablePoints++;
            return true; // 退點成功
        }
        return false; // 退點失敗
    }

    /// <summary>
    /// 取得玩家目前在特定節點上投入的點數
    /// </summary>
    public int GetAllocatedPoints(UpgradeNodeData node)
    {
        return allocatedPoints.ContainsKey(node) ? allocatedPoints[node] : 0;
    }

    /// <summary>
    /// 預覽/結算所有節點加總起來的最終屬性加成
    /// </summary>
    public Dictionary<StatType, float> PreviewAllStats()
    {
        Dictionary<StatType, float> finalStats = new Dictionary<StatType, float>();

        // 遍歷所有有投資點數的節點
        foreach (var kvp in allocatedPoints)
        {
            UpgradeNodeData node = kvp.Key;
            int pointsInvested = kvp.Value;

            // 取得這個單一節點提供的屬性加成
            var nodeBonus = node.CalculateTotalBonus(pointsInvested);

            // 將加成合併到總表 (finalStats) 中
            foreach (var bonusKvp in nodeBonus)
            {
                if (!finalStats.ContainsKey(bonusKvp.Key))
                {
                    finalStats[bonusKvp.Key] = 0f;
                }

                finalStats[bonusKvp.Key] += bonusKvp.Value;
            }
        }

        return finalStats;
    }

    /// <summary>
    /// 重置所有加點
    /// </summary>
    public void ResetAllPoints()
    {
        foreach (var points in allocatedPoints.Values)
        {
            AvailablePoints += points; // 把點數加回可用點數
        }
        allocatedPoints.Clear();
    }
}