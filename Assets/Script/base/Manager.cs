using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // 單例實例
    public static Manager Instance { get; private set; }

    // 環境實體管理器
    public EnvEntityManager EnvEntityManager { get; private set; }

    // 時間管理器
    public TickManager TickManager { get; private set; }

    // 物種總父物件
    public Transform Creature_Container { get; private set; }

    [SerializeField] private readonly Dictionary<int, Species> species = new();
    public Dictionary<int, Species> Species => species;

    void Start()
    {
        Initialize();
    }

    private void Awake()
    {
        // 確保只有一個實例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 在 Awake 中初始化 EnvEntityManager，確保在其他腳本的 Start 之前可用
        EnvEntityManager = new EnvEntityManager();
    }

    private void Initialize()
    {
        // 新建物種總父物件Creature_Container
        Creature_Container = new GameObject("Creature_Container").transform;

        // 新建 TickManager 
        TickManager = new GameObject("TickManager").AddComponent<TickManager>();

        // 啟用 EnvEntityManager 的 Tick 訂閱
        EnvEntityManager?.OnEnable();

        // 紀錄初始化完成
        LogManager.Log("[Manager] 初始化完成");
    }

    private void OnDisable()
    {
        EnvEntityManager?.OnDisable();
    }

    /// <summary>
    /// 更新天敵關係
    /// </summary>
    private void PredatorUpdate(Creature new_creature)
    {
        // 取得新生物的物種定義
        var newSpecies = new_creature.mySpecies;

        // --- 第一階段：找出誰是這隻新生物的天敵 ---
        foreach (var speciesEntry in Manager.Instance.Species.Values)
        {
            // 如果這個物種的獵物清單包含新生物的 ID，那牠就是天敵
            if (speciesEntry.preyIDList.Contains(newSpecies.speciesID))
            {
                // 將該物種 ID 加入新生物的天敵清單（如果還沒加過）
                if (!new_creature.predatorIDList.Contains(speciesEntry.speciesID))
                {
                    new_creature.predatorIDList.Add(speciesEntry.speciesID);
                }
            }
        }

        // --- 第二階段：告知新生物的獵物，新生物是牠們的天敵 ---
        foreach (var preyID in newSpecies.preyIDList)
        {
            // 找目標獵物物種
            if (Manager.Instance.Species.TryGetValue(preyID, out var preySpecies))
            {
                // 遍歷該物種的所有生物
                foreach (var preyCreature in preySpecies.creatures.Values)
                {
                    // 告訴獵物：新生物這個物種是你的天敵
                    if (!preyCreature.predatorIDList.Contains(newSpecies.speciesID))
                    {
                        preyCreature.predatorIDList.Add(newSpecies.speciesID);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 註冊新生物
    /// </summary>
    public void RegisterCreature(Creature newCreature)
    {
        int id = newCreature.speciesID;

        // 嘗試取得物種資料
        if (!species.TryGetValue(id, out var speciesData))
        {
            // 這是新物種
            speciesData = newCreature.mySpecies;
            species.Add(id, speciesData);

            // --- 自動化容器生成 ---
            // 在 EnvironmentEntities 下建立一個以物種命名的空物件
            GameObject container = new GameObject($"{speciesData.name}_Container");
            container.transform.SetParent(this.Creature_Container);
            speciesData.parentObject = container.transform;

            // 甚至可以把這個 Transform 存進 Species 物件中（如果 Species 有預留欄位）
            // speciesData.runtimeContainer = container.transform; 

            Debug.Log($"[Manager] 註冊新物種並建立容器: {speciesData.name}");
        }

        // 加入字典
        if (!speciesData.creatures.TryAdd(newCreature.UUID, newCreature))
        {
            return;
        }

        PredatorUpdate(newCreature);
    }

    /// <summary>
    /// 註銷已死亡的生物
    /// </summary>
    public void UnregisterCreature(Creature deadCreature)
    {
        int id = deadCreature.speciesID;

        if (species.TryGetValue(id, out var speciesData))
        {
            if (speciesData.creatures.Remove(deadCreature.UUID))
            {
                // 只有在移除成功後才做清理邏輯
                // 例如：清理該生物的 CD 字典或狀態，避免物件池回收後殘留舊資料
                // deadCreature.OnRecycle(); 
            }
        }
        else
        {
            Debug.LogWarning($"[Manager] 嘗試註銷不存在物種的生物: {id}");
        }
    }
}
