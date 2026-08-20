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

    public IAttribute GetAttribute(Type attributeType)
    {
        return _attributes.TryGetValue(attributeType, out IAttribute attribute) ? attribute : null;
    }

    public void AddAttribute(IAttribute attribute)
    {
        _attributes[attribute.GetType()] = attribute;
    }

    //TODO: 把各種屬性包含運行中的數據包裝成DTO像是ToCreatureAttribute()類似
    //TODO: Design Pattern: Strategy Pattern、State Pattern
    public Species species;
    public ActionStateMachine actionStateMachine;
    public string UUID;
    // --- 物種資料引用 (從 ScriptableObject 抓取，不佔個體空間) ---
    public int speciesID => species.speciesID;
    public CreatureBase creatureBase => species.creatureBase;

    public List<ActionType> actionList => species.actionList;
    public Dictionary<ActionType, int> actionMaxCD => species.actionMaxCD;
    public float variation => species.variation;


    // --- 個體遺傳屬性 ---
    public float size;
    public float speed;
    public float reproductionRate;
    public float perceptionRange;
    // public float healthRegeneration;

    // --- 運行時動態狀態 ---
    //TODO: 邊界處理直接在這邊做
    public HungerAttr hunger;
    public HealthAttr health;
    public AgeAttr age;
    public int actionCooldown;
    public bool isDead = false;
    public bool isInvincible = false;
    public bool isStunned = false;
    public bool isMoving = false;
    public float stunTimer;
    public ActionType currentAction;
    public BodyType currentBodyType;
    public Direction underAttackDirection;
    public LifeState currentLifeState;
    public Dictionary<ActionType, int> actionCD = new();
    public MovementAttr movement;
}