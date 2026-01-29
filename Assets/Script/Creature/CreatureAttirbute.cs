using System.Collections.Generic;
using UnityEngine;
using static Perception;

public partial class Creature : MonoBehaviour, ITickable {
    public Species mySpecies;
    public string UUID { get; private set; }
    // --- 物種資料引用 (從 ScriptableObject 抓取，不佔個體空間) ---
    public int speciesID => mySpecies.speciesID;
    public CreatureBase creatureBase => mySpecies.creatureBase;
    public List<int> preyIDList => mySpecies.preyIDList;
    public List<int> predatorIDList => mySpecies.predatorIDList;
    public List<ActionType> actionList => mySpecies.actionList;
    public List<FoodType> foodTypes => mySpecies.foodTypes;
    public Dictionary<ActionType, int> actionMaxCD => mySpecies.actionMaxCD;
    public float variation => mySpecies.variation;

    // --- 個體遺傳屬性 ---
    public float size { get; private set; }
    public float speed { get; private set; }
    public float maxHealth { get; private set; }
    public float reproductionRate { get; private set; }
    public float attackPower { get; private set; }
    public float lifespan { get; private set; }
    public float perceptionRange { get; private set; }
    public int sleepingHead { get; private set; }
    public int sleepingTail { get; private set; }
    public float hungerRate { get; private set; }
    public float maxHunger { get; private set; }
    public float healthRegeneration { get; private set; }
    public int sleepTime { get; private set; }

    // --- 運行時動態狀態 ---
    public float hunger { get; private set; }
    public float health { get; private set; }
    public float age { get; private set; }
    public int actionCooldown { get; private set; }
    public ActionType currentAction { get; private set; }
    public BodyType body { get; private set; }

    public Direction underAttackDirection { get; private set; }
    public Dictionary<ActionType, int> actionCD { get; private set; } = new();

    public void AttributeInheritance(Species species, CreatureAttributes creatureAttributes, GameObject creature_object)
    {
        float variationFactor() => UnityEngine.Random.Range(-species.variation, species.variation);
        //睡眠時間變異
        int delta_sleep_time() => (int)((creatureAttributes.sleeping_tail - creatureAttributes.sleeping_head) * variationFactor());
        sleepingHead = creatureAttributes.sleeping_head + delta_sleep_time();
        sleepingTail = creatureAttributes.sleeping_tail + delta_sleep_time();
        //其他玩家屬性變異
        size = creatureAttributes.size + creatureAttributes.size * variationFactor();
        speed = creatureAttributes.speed + creatureAttributes.speed * variationFactor();
        maxHealth = creatureAttributes.max_health + creatureAttributes.max_health * variationFactor();
        reproductionRate = creatureAttributes.reproduction_rate + creatureAttributes.reproduction_rate * variationFactor();
        attackPower = creatureAttributes.attack_power + creatureAttributes.attack_power * variationFactor();
        lifespan = creatureAttributes.lifespan + creatureAttributes.lifespan * variationFactor();
        perceptionRange = creatureAttributes.perception_range + creatureAttributes.perception_range * variationFactor();
        //計算衍生屬性
        sleepTime = sleepingTail - sleepingHead;
        hungerRate = AttributesCalculator.CalculateHungerRate(size, speed, attackPower);
        maxHunger = AttributesCalculator.CalculateMaxHunger(size, maxHealth, foodTypes);
        healthRegeneration = AttributesCalculator.CalculateHealthRegeneration(maxHealth, size, sleepTime);
        //初始狀態
        hunger = maxHunger;
        health = maxHealth;
        age = 0;
        actionCooldown = 0;
    }

    public CreatureAttributes ToCreatureAttribute()
    {
        CreatureAttributes attributes = new CreatureAttributes();
        attributes.size = size;
        attributes.max_health = maxHealth;
        attributes.speed = speed;
        attributes.attack_power = attackPower;
        attributes.reproduction_rate = reproductionRate;
        attributes.lifespan = lifespan;
        attributes.perception_range = perceptionRange;
        attributes.sleeping_head = sleepingHead;
        attributes.sleeping_tail = sleepingTail;
        return attributes;
    }
    public void ResetActionCooldown(ActionType actionType)
    {
        if (isDead) return;

        if (actionMaxCD.TryGetValue(actionType, out int maxCD))
        {
            actionCD[actionType] = maxCD;
        }
        else
        {
            // 如果開發者在編輯器沒設定 CD，給予警告並設為預設值 0，程式才不會斷掉
            Debug.LogWarning($"[Creature] {mySpecies.name} 缺少動作 {actionType} 的 CD 設定！");
            actionCD[actionType] = 0;
        }

        actionCooldown = constantData.UNIVERSAL_ACTION_COOLDOWN;
    }

    public int GetActionCooldown(ActionType actionType)
    {
        if (actionCD.ContainsKey(actionType))
        {
            return actionCD[actionType];
        }
        return 0;
    }
    public int GetMaxActionCooldown(ActionType actionType)
    {
        if (actionMaxCD.ContainsKey(actionType))
        {
            return actionMaxCD[actionType];
        }
        return 0;

    }
    public Dictionary<ActionType, int> GetActionCDList()
    {
        return actionCD;
    }
    public Dictionary<ActionType, int> GetActionMaxCDList()
    {
        return actionMaxCD;
    }
    public void RestoreHunger(float nutritionalValue) { 
        hunger = Mathf.Min(hunger + nutritionalValue, maxHunger);
    }
    public void SetCurrentAction(ActionType type)
    {
        currentAction = type;
    }
    //----
    public void SetHunger(float value) {
        hunger = Mathf.Clamp(value, 0, maxHunger);
    }
    public void SetHealth(float value)
    {
        health = Mathf.Clamp(value, 0, maxHealth);
    }
    public void SetAge(float value)
    {
        age = Mathf.Clamp(value, 0, lifespan);
    }
}
