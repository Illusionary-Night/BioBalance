using System;
using UnityEngine;

// 定義你的生物有哪些屬性可以被升級
// 你可以隨時在這裡新增更多屬性，完全不需要改動後面的邏輯程式碼
// 生命、速度、大小、壽命、變異、清醒時間、繁殖率、飢餓消耗、最大飽食、攻擊力、視野、回血量
public enum StatType
{
    HP,          // 最大生命值
    Speed,          // 移動速度
    Age,         // 最大壽命
    Attack,    // 攻擊力
}

[Serializable]
public class StatModifier
{
    [Tooltip("要修改的目標屬性")]
    public StatType TargetStat;

    [Tooltip("X軸: 投入的天賦點數 / Y軸: 獲得的實際加成數值")]
    public AnimationCurve GrowthCurve;

    /// <summary>
    /// 根據投入的點數，計算出該屬性應得的加成數值
    /// </summary>
    /// <param name="pointsInvested">目前投入的點數</param>
    /// <returns>增加的數值</returns>
    public float GetValueAtLevel(int pointsInvested)
    {
        // 如果沒有投入點數，回傳 0 加成
        if (pointsInvested <= 0) return 0f;

        return GrowthCurve.Evaluate(pointsInvested);
    }
}