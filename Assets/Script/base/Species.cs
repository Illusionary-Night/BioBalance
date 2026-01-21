using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Species", menuName = "BioBalance/Species")]
public class Species : ScriptableObject, ISerializationCallbackReceiver
{
    [Header("物種基本定義 (全族群統一)")]
    public int speciesID;
    public CreatureBase creatureBase;
    public List<FoodType> foodTypes = new List<FoodType>();
    public List<int> preyIDList = new List<int>();
    public List<int> predatorIDList = new List<int>();
    public List<ActionType> actionList = new List<ActionType>();
    public float variation; // 變異率

    [Header("遺傳基準值 (個體變異的起點)")]
    // 將原本屬性結構中的基礎數值放在這裡作為「範本」
    public float baseSize = 1.0f;
    public float baseSpeed = 5.0f;
    public float baseMaxHealth = 100.0f;
    public float baseReproductionRate = 0.5f;
    public float baseAttackPower = 10.0f;
    public float baseLifespan = 2000.0f;
    public float basePerceptionRange = 10.0f;

    [Header("生理時鐘基準")]
    public int baseSleepingHead = 200;
    public int baseSleepingTail = 600;

    [Header("運行時數據 (Runtime)")]
    // 執行時才產生的 Dictionary，不需要序列化
    public Dictionary<string, Creature> creatures = new();
    public int Count => creatures.Count;

    // --- 字典序列化處理 (Action Max CD) ---
    public Dictionary<ActionType, int> actionMaxCD = new();

    [SerializeField, HideInInspector] private List<ActionType> _cdKeys = new List<ActionType>();
    [SerializeField, HideInInspector] private List<int> _cdValues = new List<int>();

    public void OnBeforeSerialize()
    {
        _cdKeys.Clear();
        _cdValues.Clear();
        if (actionMaxCD == null) return;

        foreach (var kvp in actionMaxCD)
        {
            _cdKeys.Add(kvp.Key);
            _cdValues.Add(kvp.Value);
        }
    }

    public void OnAfterDeserialize()
    {
        actionMaxCD = new Dictionary<ActionType, int>();
        for (int i = 0; i != Mathf.Min(_cdKeys.Count, _cdValues.Count); i++)
        {
            actionMaxCD.Add(_cdKeys[i], _cdValues[i]);
        }
    }
    public CreatureAttributes ToCreatureAttributes()
    {
        CreatureAttributes attributes = new CreatureAttributes();
        attributes.size = baseSize;
        attributes.max_health = baseMaxHealth;
        attributes.speed = baseSpeed;
        attributes.attack_power = baseAttackPower;
        attributes.reproduction_rate = baseReproductionRate;
        attributes.lifespan = baseLifespan;
        attributes.perception_range = basePerceptionRange;
        attributes.sleeping_head = baseSleepingHead;
        attributes.sleeping_tail = baseSleepingTail;
        return attributes;
    }
}