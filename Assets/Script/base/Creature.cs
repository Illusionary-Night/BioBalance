using System.Collections;
using System.Collections.Generic;
using System;
//using Unity.VisualScripting;
using UnityEngine;


public class Creature : MonoBehaviour, Tickable
{
    // 玩家決定
    [Header("=== 玩家決定 ===")]
    [SerializeField] private int species_ID;
    public int SpeciesID { get => species_ID; set => species_ID = value; }
    [SerializeField] private float size;
    public float Size { get => size; set => size = value; }
    [SerializeField] private string _UUID;
    public string UUID { get => _UUID; }
    [SerializeField] private float speed;
    public float Speed { get => speed; set => speed = value; }
    [SerializeField] private float base_health; 
    public float BaseHealth { get => base_health; set => base_health = value; }
    [SerializeField] private float reproduction_rate;
    public float ReproductionRate { get => reproduction_rate; set => reproduction_rate = value; }
    [SerializeField] private float attack_power; 
    public float AttackPower { get => attack_power; set => attack_power = value; }
    [SerializeField] private float lifespan; 
    public float Lifespan { get => lifespan; set => lifespan = value; }
    [SerializeField] private float variation;
    public float Variation { get => variation; set => variation = value; }
    [SerializeField] private List<int> prey_ID_list = new List<int>();
    public List<int> PreyIDList { get => prey_ID_list; set => prey_ID_list = value; }       //新增食物列表
    [SerializeField] private List<int> predator_ID_list = new List<int>();
    public List<int> PredatorIDList { get => predator_ID_list; set => predator_ID_list = value; }   //新增天敵列表
    [SerializeField] private List<ActionType> action_list;
    public List<ActionType> ActionList { get => action_list; set => action_list = value; }

    [SerializeField] private int[] sleepingCycle;
    public int[] SleepingCycle { get => sleepingCycle; set => sleepingCycle = value; }

    [SerializeField] private float perceptionRange;  // 感知範圍
    public float PerceptionRange { get => perceptionRange; set => perceptionRange = value; }

    // 電腦計算
    [Header("=== 電腦計算 ===")]
    [SerializeField] private float hungerRate;
    public float HungerRate { get => hungerRate; set => hungerRate = value; }

    [SerializeField] private float maxHunger;
    public float MaxHunger { get => maxHunger; set => maxHunger = value; }

    [SerializeField] private float reproductionInterval;
    public float ReproductionInterval { get => reproductionInterval; set => reproductionInterval = value; }

    [SerializeField] private float healthRegeneration;
    public float HealthRegeneration { get => healthRegeneration; set => healthRegeneration = value; }

    [SerializeField] private List<FoodType> foodTypes;
    public List<FoodType> FoodTypes { get => foodTypes; set => foodTypes = value; }

    [SerializeField] private BodyType body;
    public BodyType Body { get => body; set => body = value; }

    [SerializeField] private int sleepTime;
    public int SleepTime { get => sleepTime; set => sleepTime = value; }

    // 當前狀態
    [Header("=== 當前狀態 ===")]
    [SerializeField] private float hunger;
    public float Hunger { get => hunger; set => hunger = value; }

    [SerializeField] private float health;
    public float Health { get => health; set => health = value; }

    [SerializeField] private float age;
    public float Age { get => age; set => age = value; }

    [SerializeField] private float reproductionCooldown;
    public float ReproductionCooldown { get => reproductionCooldown; set => reproductionCooldown = value; }

    [SerializeField] private int actionCooldown;
    public int ActionCooldown { get => actionCooldown; set => actionCooldown = value; }

    public void Initialize(CreatureAttributes creatureAttributes , GameObject creature_object)
    {
<<<<<<< HEAD
        Debug.Log("initialize");
=======
>>>>>>> main
        //個體編號
        _UUID = System.Guid.NewGuid().ToString();
        float variationFactor() => UnityEngine.Random.Range(-creatureAttributes.variation, creatureAttributes.variation);
        //睡眠時間變異
        int delta_sleep_time() => (int)((creatureAttributes.sleeping_cycle[1]-creatureAttributes.sleeping_cycle[0]) * variationFactor());
        SleepingCycle = new int[2];
        SleepingCycle[0] = creatureAttributes.sleeping_cycle[0] + delta_sleep_time();
        SleepingCycle[1] = creatureAttributes.sleeping_cycle[1] + delta_sleep_time();
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
        //計算衍生屬性
        SleepTime = SleepingCycle[1] - SleepingCycle[0];
        HungerRate = AttributesCalculator.CalculateHungerRate(Size, Speed, AttackPower);
        MaxHunger = AttributesCalculator.CalculateMaxHunger(Size, BaseHealth, FoodTypes);
        ReproductionInterval = AttributesCalculator.CalculateReproductionInterval(Size, BaseHealth);
        HealthRegeneration = AttributesCalculator.CalculateHealthRegeneration(BaseHealth, Size, SleepTime);
        //初始狀態
        Hunger = MaxHunger;
        Health = BaseHealth;
        Age = 0;
        ReproductionCooldown = 0;
        ActionCooldown = 0;
    }
    public void DoAction()
    {
        List<KeyValuePair<ActionType,float>> available_actions = new();
        //每回合開始(每生物流程)	
        //計算每個action的條件達成與否
        //計算每個達成條件的action的權重
        for (int i = 0; i < ActionList.Count; i++)
        {
            if (ActionSystem.IsConditionMet(this, ActionList[i]))
            {
                available_actions.Add(new KeyValuePair<ActionType,float>(ActionList[i], ActionSystem.GetWeight(this,ActionList[i])));
            }
        }
        //將條件達成的action進行權重排序
        available_actions.Sort((x, y) => y.Value.CompareTo(x.Value));
        while (available_actions.Count > 0)
        {
            //選擇權重最高
            ActionType selectedAction = available_actions[0].Key;
            //骰成功率
            if (ActionSystem.IsSuccess(this,selectedAction))
            {
                ActionSystem.Execute(this, selectedAction);

                return;
            }
            else
            {
                //失敗    找權重次高
                available_actions.RemoveAt(0);
            }

        }
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
        attributes.sleeping_cycle = SleepingCycle;
        attributes.FoodTypes = FoodTypes;
        attributes.Body = Body;
        attributes.prey_ID_list = PreyIDList;
        attributes.predator_ID_list = PredatorIDList;
        attributes.action_list = ActionList;
        return attributes;
    }
    public void OnTick()
    {
        //回血、餓死、老死、繁殖冷卻
        //回血
        if (Health < BaseHealth)
        {
            Health += HealthRegeneration;
        }
        Health = Mathf.Min(Health, BaseHealth);
        //餓死
        Hunger -= HungerRate;
        if (Hunger <= 0)
        {
            //Debug.Log("餓死");
        }
        //老死
        Age += 1;
        if (Age >= Lifespan)
        {
            //Debug.Log("老死");
        }
        //繁殖冷卻
        if (ReproductionCooldown > 0)
        {
            ReproductionCooldown -= 1;
        }

        //行動冷卻
        if (ActionCooldown > 0)
        {
            ActionCooldown -= 1;
        }

        if (ActionCooldown <= 0)
        {
            DoAction();
        }
    }
    // 巢狀類別：專門負責移動邏輯
    private class Movement
    {
        private Creature owner;
        private Queue<Vector2Int> path = new();

        //public Movement(Creature owner)
        //{
        //    this.owner = owner;
        //}

        //public void SetPath(IEnumerable<Vector2Int> newPath)
        //{
        //    path = new Queue<Vector2Int>(newPath);
        //}

        //public void Update()
        //{
        //    if (path.Count == 0) return;

        //    var next = path.Peek();
        //    owner.Position = next;
        //    path.Dequeue();
        //}
        //private bool TempTransformPosition(List<Vector2Int> path)
        //{
        //    // 在這裡添加位置轉換的邏輯
        //    return true;
        //}
        //private Vector2Int GetCurrentPosition()
        //{
        //    Vector3 position3D = owner.gameObject.transform.position;

        //}
        // 導航 輸入目標座標 權重圖
        //private void navigation(Vector2Int destination, TerrainMap map)
        //{
        //    List<Vector2Int> path = AStar.FindPath(currentPosition, newPosition, TerrainGenerator.Instance.GetDefinitionMap().GetTerrainWeight);
        //}

    }
}
