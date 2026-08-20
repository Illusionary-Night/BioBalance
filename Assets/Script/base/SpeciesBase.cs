using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct StatLimit
{
    public float min;
    public float max;

    // 便利的輔助方法：用來把傳入的數值強制夾在這個區間內
    public float Clamp(float value) => Mathf.Clamp(value, min, max);
}

[CreateAssetMenu(fileName = "New SpeciesBase", menuName = "BioBalance/SpeciesBase")]
public class SpeciesBase : ScriptableObject
{
    [Header("基礎識別")]
    public SpeciesBaseType speciesBaseType;
    public string description = "物種原型的基本描述";

    [Header("視覺與物理原型")]
    public GameObject defaultVisualTemplate;

    [Header("必要基因")]
    [Tooltip("只要是這個物種基底，就絕對必須擁有的行為能力")]
    public List<ActionType> requiredActions = new List<ActionType>();

    [Header("數值限制")]
    [Tooltip("企劃在設計變種時，各項基礎數值的上下限範圍")]
    public StatLimit sizeLimit = new StatLimit { min = 0.5f, max = 2.0f };
    public StatLimit speedLimit = new StatLimit { min = 1.0f, max = 10.0f };
    public StatLimit maxHealthLimit = new StatLimit { min = 50.0f, max = 500.0f };
    public StatLimit reproductionRateLimit = new StatLimit { min = 0.1f, max = 1.0f };
    public StatLimit attackPowerLimit = new StatLimit { min = 0.0f, max = 50.0f };
    public StatLimit lifespanLimit = new StatLimit { min = 1000.0f, max = 5000.0f };
    public StatLimit perceptionRangeLimit = new StatLimit { min = 5.0f, max = 30.0f };
}