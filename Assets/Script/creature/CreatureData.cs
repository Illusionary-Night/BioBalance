using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using static Perception;

public class CreatureData
{
    //TODO: 把各種屬性包含運行中的數據包裝成DTO像是ToCreatureAttribute()類似
    //TODO: Design Pattern: Strategy Pattern、State Pattern
    public Species species;
    private ActionStateMachine actionStateMachine;
    public string UUID;
    // --- 物種資料引用 (從 ScriptableObject 抓取，不佔個體空間) ---
    //TODO: lambda表達式有辦法設定get set嗎？這樣感覺有點危險
    public int speciesID => species.speciesID;
    public CreatureBase creatureBase => species.creatureBase;
    public List<int> preyIDList => species.preyIDList;
    public List<int> predatorIDList => species.predatorIDList;
    public List<ActionType> actionList => species.actionList;
    public List<FoodType> foodTypes => species.foodTypes;
    public Dictionary<ActionType, int> actionMaxCD => species.actionMaxCD;
    public float variation => species.variation;

    // --- 個體遺傳屬性 ---
    public float size;
    public float speed;
    public float maxHealth;
    public float reproductionRate;
    public float lifespan;
    public float perceptionRange;
    //public int sleepingHead ;
    //public int sleepingTail ;
    public float hungerRate;
    public float maxHunger;
    public float healthRegeneration;
    //public int sleepTime ;

    // --- 運行時動態狀態 ---
    //TODO: 邊界處理直接在這邊做
    public float hunger;
    public float health;
    public float age;
    public int actionCooldown;
    public float stunTimer = 0f;
    public bool isSleeping = false;
    public bool isDead = false;
    public bool isInvincible = false;
    public bool isStunned = false;
    public ActionType currentAction;
    public BodyType currentBodyType;
    public Direction underAttackDirection;
    public LifeState currentLifeState;
    public Dictionary<ActionType, int> actionCD = new();
    public Creature enemy;
    public CreatureMovementState movementState;


    public bool isMoving => movementState == CreatureMovementState.Walk || movementState == CreatureMovementState.Run;
    public Vector2 position;
    public Vector2Int destination;         // 格座目標（整數格）
    public List<Vector2> path = null;      // 導航後的世界座標點 (連續)
    public int currentPathIndex = 0;
    public bool awake;
    // 移動完成事件
    public event System.Action<Vector2Int> OnMovementComplete;
    public void ClearMovementEvents()
    {
        OnMovementComplete = null;
    }
    // 以下是ACTION的ATTRIBUTE


    public float attackPower;
    public Gender gender;
    //public Creature matingPartner; // 目前鎖定的配對對象
    public float reproductionCD; // 成功繁殖的冷卻
    public string fatherID;
    public string motherID;
    // 儲存六色比例，和必須為 1
    // 索引：0:紅, 1:橙, 2:黃, 3:綠, 4:藍, 5:紫
    public float[] colorGenes = new float[6];

    // 狀態判定
    private bool isWandering = false;
    public float fadeSpeed = 0.1f; // 褪色速度
    public bool isUsingColorGenes = true; // 是否啟用顏色基因影響外觀
    // 渲染相關
    private Renderer myRenderer;
    private MaterialPropertyBlock propBlock;

    [Header("Wandering Detection")]
    public float checkInterval = 1.0f;     // 判斷頻率：每 1 秒判斷一次即可
    private float _checkTimer = 0f;

    public float detectionRadius = 15.0f;   // 感應範圍
    public int minFamilyNeighbors = 1;     // 至少需要幾個「相似」同伴才不算走散
    public float similarityThreshold = 0.8f; // 基因差異容忍度 (數值越小，判斷越嚴格)

    // 強烈建議：將所有生物放在同一個 Layer (例如 "Creature")
    // 這樣 Physics2D 就不會去掃描地形或其他無關的物件
    public LayerMask creatureLayer;
}


