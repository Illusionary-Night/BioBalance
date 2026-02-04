using System.Collections.Generic;
using UnityEngine;
using static Perception;

public partial class Creature : MonoBehaviour, ITickable {
    public Species mySpecies;
    private ActionStateMachine actionStateMachine;
    private Movement movement;
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
    public bool isSleeping { get; private set; } = false;
    public bool isDead { get; private set; } = false;
    public bool isInvincible { get; private set; } = false;
    public ActionType currentAction { get; private set; }
    public BodyType currentBodyType { get; private set; }
    public Direction underAttackDirection { get; private set; }
    public LifeState currentLifeState { get; private set; }
    public Dictionary<ActionType, int> actionCD { get; private set; } = new();
}
