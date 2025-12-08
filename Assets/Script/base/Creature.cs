using System.Collections;
using System.Collections.Generic;
using System;
//using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using System.IO;


public class Creature : MonoBehaviour, ITickable
{
    private Movement movement;
    private ActionStateMachine actionStateMachine;

    // 移動完成事件
    public event System.Action<Vector2Int> OnMovementComplete;

    // 防止重複銷毀和訪問已銷毀物件的標記
    private bool isDead = false;
    public bool IsDead => isDead;

    // 當前狀態
    [SerializeField] private ActionType currentAction;
    public ActionType CurrentAction { get => currentAction; set => currentAction = value; }

    [SerializeField] private List<ActionType> weightedActionList = new List<ActionType>();
    public List<ActionType> WeightedActionList => weightedActionList;

    private Dictionary<ActionType, int> actionCD = new();
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

    [SerializeField] private int sleeping_head;
    public int SleepingHead;

    [SerializeField] private int sleeping_tail;
    public int SleepingTail;


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

    //[SerializeField] private List<ActionType> weightedActionList = new List<ActionType>();

    public void Initialize(CreatureAttributes creatureAttributes, GameObject creature_object)
    {
        //個體編號
        _UUID = System.Guid.NewGuid().ToString();
        isDead = false;
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
        FoodTypes = creatureAttributes.foodTypes;
        //計算衍生屬性
        SleepTime = SleepingTail - SleepingHead;
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
        //角色物件調適
        transform.localScale = new Vector3(size * constantData.NORMALSIZE, size * constantData.NORMALSIZE, 1f);
        movement = new Movement(this);
        // 初始化狀態機
        actionStateMachine = new ActionStateMachine(this);

        OnEnable();
    }
    public void DoAction()
    {
        if (isDead) return;
        // 委派給狀態機處理
        actionStateMachine.EvaluateAndExecute();
    }
    /// <summary>
    /// 取得狀態機實例（供 Action 使用）
    /// </summary>
    public ActionStateMachine GetStateMachine()
    {
        return actionStateMachine;
    }

    public void SetActionCooldown(ActionType actionType, int cooldown)
    {
        if (isDead) return;
        actionCD[actionType] = cooldown;
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
        return attributes;
    }
    public void OnEnable()
    {
        Manager.OnTick += OnTick;
    }
    public void OnDisable()
    {
        Manager.OnTick -= OnTick;
    }
    public void Die()
    {
        // 防止重複執行
        if (isDead) return;
        isDead = true;

        //Debug.LogWarning("Using Manager instance from: " + Manager.Instance.gameObject.name);
        //Debug.LogWarning("MeatPrefab is: " + Manager.Instance.MeatPrefab);

        // 重要：先取消訂閱事件
        OnDisable();

        if (Manager.Instance != null)
        {
            // Spawn food item
            GameObject meat_prefab = Resources.Load<GameObject>("Prefabs/Edible/Meat");
            Instantiate(meat_prefab, transform.position, Quaternion.identity, Manager.Instance.EnvironmentEntities)
                .GetComponent<Edible>()
                .Initialize(Vector2Int.RoundToInt(transform.position));
        }
        else
        {
            //Debug.LogWarning("MeatPrefab is null");
        }
        
        if (Manager.Instance != null)
        {
            Manager.Instance.UnregisterCreature(this);
        }
        
        Destroy(gameObject);
    }
    public void OnTick()
    {
        // 安全檢查
        if (isDead || this == null) return;

        //test
        if (movement != null && movement.path != null)
        {
            Debug.Log("path exist: " + (movement.path != null));
            Debug.Log("Des:" + movement.GetDestination());
        }
        
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
            Die();
            return; // 立即返回
        }
        
        //老死
        Age += 1;
        if (Age >= Lifespan)
        {
            Die();
            return; // 立即返回
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

        foreach (var key in actionCD.Keys.ToList())
        {
            if (actionCD[key] > 0)
            {
                actionCD[key] -= 1;
            }
        }

        if (ActionCooldown <= 0)
        {
            DoAction();
        }
        
        if (movement != null)
        {
            movement.MoveOnTick();
        }
    }
    // 巢狀類別：Movement
    private class Movement
    {
        private Creature owner;
        private Rigidbody2D rb;                 // 優先使用物理剛體
        private Vector2Int Destination;         // 格座目標（整數格）
        public List<Vector2> path = null;      // 導航後的世界座標點 (連續)
        private int currentPathIndex = 0;
        private float stuckThreshold = 0.2f;    // 偵測被擠走/卡住的容忍距離
        private int stuckLimitTicks = 6;        // 超過幾次就重新導航
        private int stuckCounter = 0;
        private Vector2 lastRecordedPosition;
        private bool awake;

        public Movement(Creature owner)
        {
            this.owner = owner;
            this.rb = owner.GetComponent<Rigidbody2D>(); // 可能為 null
                                                         // 初始化 lastRecordedPosition 為真實位置（權威）
            lastRecordedPosition = GetAuthoritativePosition();
            awake = false;
        }

        // 設定目的地（格座）
        public void SetDestination(Vector2Int destination)
        {
            // 安全檢查
            if (owner == null || owner.isDead) return;
            
            //Debug.Log("SetDestination");
            Destination = destination;
            awake = true;
            Navigate();
        }

        void PreventDrifting()
        {
            if (rb.linearVelocity != Vector2.zero)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }

        public void MoveOnTick()
        {
            // 安全檢查
            if (owner == null || owner.isDead) return;

            Debug.Log("Distance: " + Vector2.Distance(TempGetCurrentPosition(), Destination));
            if (!awake) return;
            if (path == null)
            {
                //Debug.Log("path is null");
                return;
            }
            // 這一回合開始的真實位置
            Vector2 actualPos = GetAuthoritativePosition();
            Vector2 expectedPos = actualPos;

            PreventDrifting();

            //移動
            if (path != null && currentPathIndex < path.Count)
            {
                Vector2 target = path[currentPathIndex];

                // 計算下一個位置
                Vector2 nextPos = Vector2.MoveTowards(actualPos, target, owner.Speed * Time.fixedDeltaTime);
                rb.MovePosition(nextPos);
                expectedPos = nextPos;  // 記起來

                // 修正：方向應該根據 nextPos，而不是 actualPos
                Vector2 direction = (nextPos - actualPos).normalized;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    owner.transform.rotation = Quaternion.Euler(0, 0, angle);
                }

                // 判斷是否抵達目標
                if (Vector2.Distance(nextPos, target) < 0.05f)
                    currentPathIndex++;
            }
            else if (awake && path != null && currentPathIndex >= path.Count)
            {
                // 路徑走完，檢查是否到達目的地
                Vector2Int currentPos = TempGetCurrentPosition();
                Debug.Log("Distance: " + Vector2.Distance(currentPos, Destination));
                if (Vector2.Distance(currentPos, Destination) < 1.5f)
                {
                    Debug.Log("Path end");
                    awake = false;
                    owner.OnMovementComplete?.Invoke(Destination);
                }
            }
            else
            {
                Debug.Log("awake/path is null" + awake + " " + path);
            }

            // 正確做法：記錄「預期」的移動位置
            lastRecordedPosition = expectedPos;
        }


        // 導航（呼叫你的 A* 或其它尋路系統）
        public void Navigate()
        {
            // 安全檢查
            if (owner == null || owner.isDead) return;

            Debug.Log("Navigate");
            Vector2Int start = Vector2Int.RoundToInt(GetAuthoritativePosition());
            Vector2Int goal = Destination;

            // 假設 AStar.FindPath 回傳 List<Vector2Int> 或 null
            // 使用 A* 演算法尋找路徑
            List<Vector2Int> rawPath = AStar.FindPath(start, goal, TerrainGenerator.Instance.GetDefinitionMap().GetTerrainWeight);
            if (rawPath == null || rawPath.Count == 0)
            {
                path = null;
                currentPathIndex = 0;
                return;
            }

            // 把格子座標轉成世界座標 (中心點)，視你的格子系統可能需要偏移
            path = rawPath.Select(v => new Vector2(v.x, v.y)).ToList();
            currentPathIndex = 0;
        }

        // 取得當前經過物理系統修正後的整數格座標（四捨五入）
        public Vector2Int TempGetCurrentPosition()
        {
            Vector2 actual = GetAuthoritativePosition();
            return Vector2Int.RoundToInt(actual);
        }

        // 取得物理/Transform 的權威位置
        private Vector2 GetAuthoritativePosition()
        {
            // 安全檢查
            if (owner == null || owner.isDead || owner.gameObject == null)
                return Vector2.zero;
                
            if (rb != null) return rb.position;
            return owner.transform.position;
        }
        public Vector2Int GetDestination()
        {
            return Destination;
        }
    }
    public void MoveTo(Vector2Int destination)
    {
        if (isDead || movement == null) return;
        movement.SetDestination(destination);
    }

    public void ForceNavigate()
    {
        if (isDead || movement == null) return;
        movement.Navigate();
    }

    public Vector2Int GetRoundedPosition()
    {
        if (movement == null) return Vector2Int.zero;
        return movement.TempGetCurrentPosition();
    }
}
