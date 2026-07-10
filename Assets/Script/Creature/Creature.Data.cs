// using System.Collections.Generic;
// using UnityEngine;
// using System;

// public partial class Creature : MonoBehaviour, ITickable
// {
//     //TODO: 這邊的屬性可以用Attribute包裝，然後用Dictionary<ActionType, Attribute>來管理
//     public List<int> preyIDList => mySpecies.preyIDList;
//     public List<int> predatorIDList => mySpecies.predatorIDList;
//     public List<FoodType> foodTypes => mySpecies.foodTypes;
//     public float attackPower { get; private set; }
//     public bool isSleeping { get; private set; } = false;
//     public Creature enemy { get; private set; }
//     public CreatureMovementState movementState { get; private set; }
//     public Gender gender;
//     //public Creature matingPartner; // 目前鎖定的配對對象
//     public float reproductionCD; // 成功繁殖的冷卻
//     public string fatherID { get; private set; }
//     public string motherID { get; private set; }

//     //TODO: 把各種屬性包含運行中的數據包裝成DTO像是ToCreatureAttribute()類似
//     //TODO: Design Pattern: Strategy Pattern、State Pattern
//     public Species mySpecies;
//     private ActionStateMachine actionStateMachine;
//     private Movement movement;
//     public string UUID { get; private set; }
//     // --- 物種資料引用 (從 ScriptableObject 抓取，不佔個體空間) ---
//     //TODO: lambda表達式有辦法設定get set嗎？這樣感覺有點危險
//     public int speciesID => mySpecies.speciesID;
//     public CreatureBase creatureBase => mySpecies.creatureBase;

//     public List<ActionType> actionList => mySpecies.actionList;
//     public Dictionary<ActionType, int> actionMaxCD => mySpecies.actionMaxCD;
//     public float variation => mySpecies.variation;


//     // --- 個體遺傳屬性 ---
//     public float size { get; private set; }
//     public float speed { get; private set; }
//     public float maxHealth { get; private set; }
//     public float reproductionRate { get; private set; }
//     public float lifespan { get; private set; }
//     public float perceptionRange { get; private set; }
//     //public int sleepingHead { get; private set; }
//     //public int sleepingTail { get; private set; }
//     public float hungerRate { get; private set; }
//     public float maxHunger { get; private set; }
//     public float healthRegeneration { get; private set; }
//     //public int sleepTime { get; private set; }

//     // --- 運行時動態狀態 ---
//     //TODO: 邊界處理直接在這邊做
//     public float hunger { get; private set; }
//     public float health { get; private set; }
//     public float age { get; private set; }
//     public int actionCooldown { get; private set; }
//     public float stunTimer { get; private set; } = 0f;
//     public bool isDead { get; private set; } = false;
//     public bool isInvincible { get; private set; } = false;
//     public bool isStunned { get; private set; } = false;
//     public ActionType currentAction { get; private set; }
//     public BodyType currentBodyType { get; private set; }
//     public Direction underAttackDirection { get; private set; }
//     public LifeState currentLifeState { get; private set; }
//     public Dictionary<ActionType, int> actionCD { get; private set; } = new();

//     // ===================================包一包==================================
//     // 儲存六色比例，和必須為 1
//     // 索引：0:紅, 1:橙, 2:黃, 3:綠, 4:藍, 5:紫
//     public float[] colorGenes = new float[6];

//     // 狀態判定
//     private bool isWandering = false;
//     public float fadeSpeed = 0.1f; // 褪色速度
//     public bool isUsingColorGenes = true; // 是否啟用顏色基因影響外觀
//     // 渲染相關
//     private Renderer myRenderer;
//     private MaterialPropertyBlock propBlock;

//     [Header("Wandering Detection")]
//     public float checkInterval = 1.0f;     // 判斷頻率：每 1 秒判斷一次即可
//     private float _checkTimer = 0f;

//     public float detectionRadius = 15.0f;   // 感應範圍
//     public int minFamilyNeighbors = 1;     // 至少需要幾個「相似」同伴才不算走散
//     public float similarityThreshold = 0.8f; // 基因差異容忍度 (數值越小，判斷越嚴格)

//     // 強烈建議：將所有生物放在同一個 Layer (例如 "Creature")
//     // 這樣 Physics2D 就不會去掃描地形或其他無關的物件
//     public LayerMask creatureLayer;
//     // ========================================================================

// }