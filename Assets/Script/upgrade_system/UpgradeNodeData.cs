using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewUpgradeNode", menuName = "BioBalance/Upgrade System/Upgrade Node")]
public class UpgradeNodeData : ScriptableObject
{
    [Header("節點基本資訊")]
    public string NodeName;             // 節點名稱，例如："強健體魄"
    [TextArea]
    public string Description;          // 節點的敘述，顯示在 UI 上用

    [Tooltip("這個節點最多可以投入幾點")]
    public int MaxAllocatablePoints = 5;

    [Header("屬性修正器 (可複選)")]
    [Tooltip("點擊這個節點會影響哪些屬性(可以設定多個)")]
    public List<StatModifier> Modifiers = new List<StatModifier>();

    /// <summary>
    /// 計算這個節點在特定點數下，提供的「所有」屬性總和
    /// </summary>
    public Dictionary<StatType, float> CalculateTotalBonus(int currentPoints)
    {
        Dictionary<StatType, float> totalBonus = new Dictionary<StatType, float>();

        foreach (var modifier in Modifiers)
        {
            // 如果字典裡還沒有這個屬性，先初始化為 0
            if (!totalBonus.ContainsKey(modifier.TargetStat))
            {
                totalBonus[modifier.TargetStat] = 0f;
            }

            // 將計算出的加成數值累加進去
            totalBonus[modifier.TargetStat] += modifier.GetValueAtLevel(currentPoints);
        }

        return totalBonus;
    }
}