using System.Collections.Generic;
using UnityEngine;

public partial class Creature : MonoBehaviour, ITickable {
    [Header("Information")]
    [SerializeField] private List<ActionType> weightedActionList = new List<ActionType>();
    public List<ActionType> WeightedActionList => weightedActionList;
    [SerializeField] private int species_ID;
    public int SpeciesID { get => species_ID; private set => species_ID = value; }

    private Dictionary<ActionType, int> actionCD = new();
    
    private Dictionary<ActionType, int> action_max_CD = new();

    [SerializeField] private string _UUID;
    public string UUID { get => _UUID; }//-------------------------------------------------------haven't use
    // Player Design---------------------------------------------------------------------------
    [Header("Player Design")]
    [SerializeField] private float size;
    public float Size { get => size; private set => size = value; }
    [SerializeField] private float speed;
    public float Speed { get => speed; private set => speed = value; }
    [SerializeField] private float base_health;
    public float BaseHealth { get => base_health; private set => base_health = value; }
    [SerializeField] private float reproduction_rate;
    public float ReproductionRate { get => reproduction_rate; private set => reproduction_rate = value; }
    [SerializeField] private float attack_power;
    public float AttackPower { get => attack_power; private set => attack_power = value; }
    [SerializeField] private float lifespan;
    public float Lifespan { get => lifespan; private set => lifespan = value; }
    [SerializeField] private float variation;
    public float Variation { get => variation; private set => variation = value; }
    [SerializeField] private List<int> prey_ID_list = new List<int>();
    public List<int> PreyIDList { get => prey_ID_list; private set => prey_ID_list = value; }       
    [SerializeField] private List<int> predator_ID_list = new List<int>();
    public List<int> PredatorIDList { get => predator_ID_list; private set => predator_ID_list = value; } 
    [SerializeField] private List<ActionType> action_list;
    public List<ActionType> ActionList { get => action_list; private set => action_list = value; }
    [SerializeField] private int sleeping_head;
    public int SleepingHead { get => sleeping_head; private set => sleeping_head = value; }
    [SerializeField] private int sleeping_tail;
    public int SleepingTail { get => sleeping_tail; private set => sleeping_tail = value; }
    [SerializeField] private float perceptionRange;
    public float PerceptionRange { get => perceptionRange; private set => perceptionRange = value; }

    [Header("Computer Calculation")]
    [SerializeField] private float hungerRate;
    public float HungerRate { get => hungerRate; private set => hungerRate = value; }

    [SerializeField] private float maxHunger;
    public float MaxHunger { get => maxHunger; private set => maxHunger = value; }

    //[SerializeField] private float reproductionInterval;
    //public float ReproductionInterval { get => reproductionInterval; private set => reproductionInterval = value; }

    [SerializeField] private float healthRegeneration;
    public float HealthRegeneration { get => healthRegeneration; private set => healthRegeneration = value; }

    [SerializeField] private List<FoodType> foodTypes;
    public List<FoodType> FoodTypes { get => foodTypes; private set => foodTypes = value; }

    [SerializeField] private BodyType body;
    public BodyType Body { get => body; private set => body = value; }

    [SerializeField] private int sleepTime;
    public int SleepTime { get => sleepTime; private set => sleepTime = value; }

    [Header("Current State")]
    [SerializeField] private float hunger;
    public float Hunger { get => hunger; set => hunger = value; }

    [SerializeField] private float health;
    public float Health { get => health; set => health = value; }

    [SerializeField] private float age;
    public float Age { get => age; set => age = value; }

    //[SerializeField] private float reproductionCooldown;
    //public float ReproductionCooldown { get => reproductionCooldown; private set => reproductionCooldown = value; }

    [SerializeField] private int actionCooldown;
    public int ActionCooldown { get => actionCooldown; set => actionCooldown = value; }

    [SerializeField] private ActionType currentAction;
    public ActionType CurrentAction { get => currentAction; set => currentAction = value; }


    public void AttributeInheritance(CreatureAttributes creatureAttributes, GameObject creature_object)
    {
        float variationFactor() => UnityEngine.Random.Range(-creatureAttributes.variation, creatureAttributes.variation);
        //睡眠時間變異
        int delta_sleep_time() => (int)((creatureAttributes.sleeping_tail - creatureAttributes.sleeping_head) * variationFactor());
        SleepingHead = creatureAttributes.sleeping_head + delta_sleep_time();
        SleepingTail = creatureAttributes.sleeping_tail + delta_sleep_time();
        //其他玩家屬性變異
        Size = creatureAttributes.size + creatureAttributes.size * variationFactor();
        Speed = creatureAttributes.speed + creatureAttributes.speed * variationFactor();
        BaseHealth = creatureAttributes.base_health + creatureAttributes.base_health * variationFactor();
        ReproductionRate = creatureAttributes.reproduction_rate + creatureAttributes.reproduction_rate * variationFactor();
        AttackPower = creatureAttributes.attack_power + creatureAttributes.attack_power * variationFactor();
        Lifespan = creatureAttributes.lifespan + creatureAttributes.lifespan * variationFactor();
        PerceptionRange = creatureAttributes.perception_range + creatureAttributes.perception_range * variationFactor();
        //其他玩家屬性不變
        SpeciesID = creatureAttributes.species_ID;
        Variation = creatureAttributes.variation;
        PreyIDList = new List<int>(creatureAttributes.prey_ID_list);
        PredatorIDList = new List<int>(creatureAttributes.predator_ID_list);
        ActionList = new List<ActionType>(creatureAttributes.action_list);
        action_max_CD = creatureAttributes.action_max_CD;
        FoodTypes = creatureAttributes.foodTypes;
        //計算衍生屬性
        SleepTime = SleepingTail - SleepingHead;
        HungerRate = AttributesCalculator.CalculateHungerRate(Size, Speed, AttackPower);
        MaxHunger = AttributesCalculator.CalculateMaxHunger(Size, BaseHealth, FoodTypes);
        //ReproductionInterval = AttributesCalculator.CalculateReproductionInterval(Size, BaseHealth);
        HealthRegeneration = AttributesCalculator.CalculateHealthRegeneration(BaseHealth, Size, SleepTime);
        //初始狀態
        Hunger = MaxHunger;
        Health = BaseHealth;
        Age = 0;
        //ReproductionCooldown = 0;
        ActionCooldown = 0;
    }

    public CreatureAttributes ToCreatureAttribute()
    {
        CreatureAttributes attributes = new CreatureAttributes();
        attributes.species_ID = SpeciesID;
        attributes.size = Size;
        attributes.base_health = BaseHealth;
        attributes.speed = Speed;
        attributes.attack_power = AttackPower;
        attributes.reproduction_rate = ReproductionRate;
        attributes.variation = Variation;
        attributes.lifespan = Lifespan;
        attributes.perception_range = PerceptionRange;
        attributes.sleeping_head = SleepingHead;
        attributes.sleeping_tail = SleepingTail;
        attributes.foodTypes = FoodTypes;
        attributes.Body = Body;
        attributes.prey_ID_list = PreyIDList;
        attributes.predator_ID_list = PredatorIDList;
        attributes.action_list = ActionList;
        attributes.action_max_CD = action_max_CD;
        return attributes;
    }
    public void ResetActionCooldown(ActionType actionType)
    {
        if (isDead) return;
        actionCD[actionType] = action_max_CD[actionType];
        actionCooldown = 20;
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
        if (action_max_CD.ContainsKey(actionType))
        {
            return action_max_CD[actionType];
        }
        return 0;

    }
    public Dictionary<ActionType, int> GetActionCDList()
    {
        return actionCD;
    }
    public Dictionary<ActionType, int> GetActionMaxCDList()
    {
        return action_max_CD;
    }
}
