using System.Collections.Generic;
using UnityEngine;
using System;


[System.Serializable]
public class CreatureData
{
    private readonly Dictionary<Type, IAttribute> _attributes = new();

    public T GetAttribute<T>() where T : class, IAttribute
    {
        return _attributes.TryGetValue(typeof(T), out IAttribute attribute) ? attribute as T : null;
    }

    public void AddAttribute(IAttribute attribute)
    {
        _attributes[attribute.GetType()] = attribute;
    }

    //TODO: 把各種屬性包含運行中的數據包裝成DTO像是ToCreatureAttribute()類似
    //TODO: Design Pattern: Strategy Pattern、State Pattern
    public Species mySpecies;
    private ActionStateMachine actionStateMachine;
    public string UUID { get; private set; }
    // --- 物種資料引用 (從 ScriptableObject 抓取，不佔個體空間) ---
    //TODO: lambda表達式有辦法設定get set嗎？這樣感覺有點危險
    public int speciesID => mySpecies.speciesID;
    public CreatureBase creatureBase => mySpecies.creatureBase;

    public List<ActionType> actionList => mySpecies.actionList;
    public Dictionary<ActionType, int> actionMaxCD => mySpecies.actionMaxCD;
    public float variation => mySpecies.variation;


    // --- 個體遺傳屬性 ---
    public float size { get; private set; }
    public float speed { get; private set; }
    public float maxHealth { get; private set; }
    public float reproductionRate { get; private set; }
    public float lifespan { get; private set; }
    public float perceptionRange { get; private set; }
    //public int sleepingHead { get; private set; }
    //public int sleepingTail { get; private set; }
    public float hungerRate { get; private set; }
    public float maxHunger { get; private set; }
    public float healthRegeneration { get; private set; }
    //public int sleepTime { get; private set; }

    // --- 運行時動態狀態 ---
    //TODO: 邊界處理直接在這邊做
    public float hunger { get; private set; }
    public float health { get; private set; }
    public float age { get; private set; }
    public int actionCooldown { get; private set; }
    public float stunTimer { get; private set; } = 0f;
    public bool isDead { get; private set; } = false;
    public bool isInvincible { get; private set; } = false;
    public bool isStunned { get; private set; } = false;
    public ActionType currentAction { get; private set; }
    public BodyType currentBodyType { get; private set; }
    public Direction underAttackDirection { get; private set; }
    public LifeState currentLifeState { get; private set; }
    public Dictionary<ActionType, int> actionCD { get; private set; } = new();
}