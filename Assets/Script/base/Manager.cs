using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }
    [SerializeField] private readonly Dictionary<int, Species> species = new();
    public Dictionary<int,Species> Species => species;
    [SerializeField] private readonly Dictionary<Vector2Int, Edible> fooditems = new();
    public Dictionary<Vector2Int, Edible> FoodItems => fooditems;
    public static event System.Action OnTick;
    public float tickInterval = 1f / 2; // 30 ticks per second
    private float tick_timer = 0;
    private int mixTickTime = 240000;
    [SerializeField] public int TickTime;
    private int tick_time => tick_time;
    public Transform EnvironmentEntities { get; private set; }
    void Start()
    {
        Initialize();
    }
    private void Awake()
    {
        // One instance only
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        tick_timer += Time.deltaTime;
        if (tick_timer >= tickInterval)
        {
            tick_timer -= tickInterval;
            TickTime = (TickTime + 1) % mixTickTime;
            // Trigger event
            OnTick?.Invoke();
        }
    }
    private void Initialize()
    {
        TickTime = 0;
        GameObject env_entites_prefab = Resources.Load<GameObject>("Prefabs/Parents/EnvironmentEntities");
        EnvironmentEntities = Instantiate(env_entites_prefab).transform;
    }

    private void OnEnable()
    {
        OnTick += SpawnEdible;
    }
    private void OnDisable()
    {
        OnTick -= SpawnEdible;
    }
    private void PredatorUpdate(Creature new_creature)
    {
        // 取得新生物的物種定義
        var newSpecies = new_creature.mySpecies;

        // --- 第一部分：找出誰是這隻新生物的天敵 ---
        foreach (var speciesEntry in Manager.Instance.Species.Values)
        {
            // 如果這個物種的獵物清單包含新生物的 ID，那它就是天敵
            if (speciesEntry.preyIDList.Contains(newSpecies.speciesID))
            {
                // 將該物種 ID 加入新生物的天敵清單 (如果還沒加過)
                if (!new_creature.predatorIDList.Contains(speciesEntry.speciesID))
                {
                    new_creature.predatorIDList.Add(speciesEntry.speciesID);
                }
            }
        }

        // --- 第二部分：告知新生物的獵物，新生物是它們的天敵 ---
        foreach (var preyID in newSpecies.preyIDList)
        {
            // 找到目標獵物物種
            if (Manager.Instance.Species.TryGetValue(preyID, out var preySpecies))
            {
                // 遍歷該物種的所有個體
                foreach (var preyCreature in preySpecies.creatures.Values)
                {
                    // 告訴獵物：新生物的物種是你的天敵
                    if (!preyCreature.predatorIDList.Contains(newSpecies.speciesID))
                    {
                        preyCreature.predatorIDList.Add(newSpecies.speciesID);
                    }
                }
            }
        }
    }
    public void RegisterCreature(Creature newCreature)
    {
        int id = newCreature.speciesID;

        // 1. 嘗試從字典中獲取現有的物種資料
        if (!species.TryGetValue(id, out var speciesData))
        {
            // 2. 如果字典裡沒有這個 ID，表示這是一個全新的物種
            // 我們直接將生物身上帶的物種引用註冊進 Manager
            speciesData = newCreature.mySpecies;
            species.Add(id, speciesData);

            Debug.Log($"[Manager] 註冊全新物種: {speciesData.name} (ID: {id})");
        }

        // 3. 將生物加入該物種的運行時字典
        // 因為 speciesData 無論是舊有的還是剛新增的，現在都保證不為 null
        if (!speciesData.creatures.TryAdd(newCreature.uuid, newCreature))
        {
            Debug.LogWarning($"[Manager] 生物 {newCreature.uuid} 已經在物種 {id} 的清單中了。");
            return;
        }

        // 4. 更新生態鏈關係（天敵/獵物）
        PredatorUpdate(newCreature);
    }
    public void UnregisterCreature(Creature deadCreature)
    {
        int id = deadCreature.speciesID;

        if (species.TryGetValue(id, out var speciesData))
        {
            if (speciesData.creatures.Remove(deadCreature.uuid))
            {
                // 只有移除成功才執行後續邏輯
                // 例如：清空該生物的 CD 字典或狀態，避免物件池回收後殘留舊資料
                // deadCreature.OnRecycle(); 
            }
        }
        else
        {
            Debug.LogWarning($"[Manager] 嘗試註銷未知物種的生物: {id}");
        }
    }
    // spawn the edible item
    private void SpawnEdible()
    {
        //Debug.Log("grass num: "+FoodItems.Count);
        //TODO: Const of max food items and map size
        // Limit the number of food items
        if (fooditems.Count > 120) return;

        Vector2Int position = new Vector2Int(
            Random.Range(200, 300),
            Random.Range(200, 300)
        );
        // Check if position is occupied
        if (fooditems.ContainsKey(position)) return;
        // Check terrain type
        var random_positions = GetRandomPosition(position, 3, Random.Range(1, 5));
        foreach (var pos in random_positions)
        {
            //Debug.Log("Trying to spawn food at: " + pos);
            //Debug.Log("Terrain at pos: " + TerrainGenerator.Instance.GetDefinitionMap().GetTerrain(pos));
            if (fooditems.ContainsKey(pos)) continue;
            if (TerrainGenerator.Instance.GetDefinitionMap().GetTerrain(pos) != TerrainType.Grass) continue;
            Vector3 pos3 = new Vector3(pos.x, pos.y, 0f);
            // Spawn food item
            GameObject food_item_prefab = Resources.Load<GameObject>("Prefabs/Edible/Grass");
            GameObject food_item_object = Instantiate(food_item_prefab, pos3, Quaternion.identity,EnvironmentEntities);
            Edible food_item = food_item_object.GetComponent<Edible>();
            fooditems.Add(pos, food_item);
            food_item.Initialize();

        }

    }

    private List<Vector2Int> GetRandomPosition(Vector2Int position, int r, int n)
    {
        List<Vector2Int> position_ = new();
        HashSet<Vector2Int> used_position = new();
        uint tries = 0;
        while (position_.Count < n && tries < n * 10)
        {
            float angle = Random.Range(0f, 360f);
            float radius = Random.Range(0f, r);
            Vector2Int new_position = position + new Vector2Int(
                Mathf.RoundToInt(radius * Mathf.Cos(angle * Mathf.Deg2Rad)),
                Mathf.RoundToInt(radius * Mathf.Sin(angle * Mathf.Deg2Rad))
            );
            if (!used_position.Contains(new_position))
            {
                used_position.Add(new_position);
                position_.Add(new_position);
            }
            tries++;
        }
        return position_;
    }
}
