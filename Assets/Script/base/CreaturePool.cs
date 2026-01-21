/*
 * ===========================================================================================
 * CreaturePool - 生物物件池管理器
 * ===========================================================================================
 * 
 * [功能說明]
 * 使用 Unity 的 ObjectPool 來管理 Creature 的生成與回收，
 * 避免頻繁的 Instantiate/Destroy 造成的效能問題與 GC 壓力。
 * 
 * -------------------------------------------------------------------------------------------
 * [公開屬性 (Properties)]
 * -------------------------------------------------------------------------------------------
 * 
 * ● CountActive (int)
 *   - 說明：目前正在場景中使用的生物數量
 *   - 用法：Debug.Log($"活躍生物: {CreaturePool.CountActive}");
 * 
 * ● CountInactive (int)
 *   - 說明：目前在池中閒置、等待被重用的生物數量
 *   - 用法：Debug.Log($"閒置生物: {CreaturePool.CountInactive}");
 * 
 * ● CountAll (int)
 *   - 說明：物件池管理的生物總數（活躍 + 閒置）
 *   - 用法：Debug.Log($"總計生物: {CreaturePool.CountAll}");
 * 
 * -------------------------------------------------------------------------------------------
 * [公開方法 (Methods)]
 * -------------------------------------------------------------------------------------------
 * 
 * ● Initialize(int preWarmCount = 0)
 *   - 說明：初始化物件池，可選擇性地預熱指定數量的生物
 *   - 參數：preWarmCount - 預先創建的生物數量（預設為 0）
 *   - 用法：
 *       // 在遊戲開始時呼叫
 *       CreaturePool.Initialize(100);  // 預先創建 100 隻生物
 * 
 * ● PreWarm(int count)
 *   - 說明：預熱物件池，提前創建指定數量的生物並放入池中待用
 *   - 參數：count - 要預先創建的生物數量
 *   - 用法：
 *       CreaturePool.PreWarm(50);  // 額外預熱 50 隻生物
 *   - 注意：適合在 Loading 畫面或場景初始化時使用，避免遊戲中突然大量生成造成卡頓
 * 
 * ● GetCreature()
 *   - 說明：從池中取得一個未初始化的 Creature
 *   - 回傳：Creature 實例（需要手動呼叫 Initialize）
 *   - 用法：
 *       Creature creature = CreaturePool.GetCreature();
 *       creature.Initialize(species, attributes, creature.gameObject);
 *       creature.transform.position = spawnPosition;
 * 
 * ● GetCreature(species, CreatureAttributes attributes, Vector3 position, Transform parent = null)
 *   - 說明：從池中取得一個已初始化的 Creature（推薦使用）
 *   - 參數：
 *       species    - 生物的種族
 *       attributes - 生物的遺傳屬性
 *       position   - 生成位置（世界座標）
 *       parent     - 父物件（可選，預設為 null）
 *   - 回傳：已初始化並放置好位置的 Creature 實例
 *   - 用法：
 *       CreatureAttributes attr = parentCreature.ToCreatureAttribute();
 *       Vector3 pos = new Vector3(100, 100, 0);
 *       Creature baby = CreaturePool.GetCreature(attr, pos);
 * 
 * ● ReleaseCreature(Creature creature)
 *   - 說明：將不再使用的 Creature 回收到物件池中
 *   - 參數：creature - 要回收的生物實例
 *   - 用法：
 *       CreaturePool.ReleaseCreature(deadCreature);
 *   - 注意：通常不需要手動呼叫，Creature.Die() 會自動處理回收
 * 
 * ● Clear()
 *   - 說明：清空整個物件池，銷毀所有閒置的生物
 *   - 用法：
 *       // 在場景切換或遊戲結束時呼叫
 *       CreaturePool.Clear();
 *   - 注意：這會銷毀所有池中的物件，但不影響場景中正在使用的生物
 * 
 * -------------------------------------------------------------------------------------------
 * [使用範例]
 * -------------------------------------------------------------------------------------------
 * 
 * // 1. 遊戲初始化時預熱
 * void Start()
 * {
 *     CreaturePool.Initialize(100);
 * }
 * 
 * // 2. 繁殖時使用物件池
 * void Reproduce(Creature parent)
 * {
 *     Vector3 spawnPos = parent.transform.position + Vector3.right;
 *     Creature baby = CreaturePool.GetCreature(parent.ToCreatureAttribute(), spawnPos);
 * }
 * 
 * // 3. 生物死亡時自動回收（已整合在 Creature.Die() 中）
 * // 不需要手動處理
 * 
 * // 4. 監控物件池狀態
 * void OnGUI()
 * {
 *     GUI.Label(new Rect(10, 10, 200, 20), 
 *         $"池狀態: {CreaturePool.CountActive}/{CreaturePool.CountAll}");
 * }
 * 
 * ===========================================================================================
 */

using UnityEngine;
using UnityEngine.Pool;

public static class CreaturePool
{
    private static GameObject prefab;
    private static Transform poolParent;
    
    // Object pool for Creature instances
    private static ObjectPool<Creature> pool = new(
        createFunc: CreateCreature,
        actionOnGet: ActionOnGet,
        actionOnRelease: ActionOnRelease,
        actionOnDestroy: ActionOnDestroy,
        collectionCheck: false,
        defaultCapacity: 200,
        maxSize: 10000
    );

    // 統計資訊
    public static int CountActive => pool.CountActive;
    public static int CountInactive => pool.CountInactive;
    public static int CountAll => pool.CountAll;

    /// <summary>
    /// 初始化物件池（可選，會自動延遲初始化）
    /// </summary>
    /// <param name="preWarmCount">預熱數量</param>
    public static void Initialize(int preWarmCount = 0)
    {
        EnsureInitialized();
        
        if (preWarmCount > 0)
        {
            PreWarm(preWarmCount);
        }
    }

    /// <summary>
    /// 預熱：提前創建一批 Creature 並放入池中
    /// </summary>
    public static void PreWarm(int count)
    {
        EnsureInitialized();
        
        Creature[] creatures = new Creature[count];
        for (int i = 0; i < count; i++)
        {
            creatures[i] = pool.Get();
        }
        for (int i = 0; i < count; i++)
        {
            pool.Release(creatures[i]);
        }
        
        Debug.Log($"CreaturePool: Pre-warmed {count} creatures. Pool size: {CountInactive}");
    }

    /// <summary>
    /// 從池中取得一個 Creature（未初始化）
    /// </summary>
    public static Creature GetCreature()
    {
        EnsureInitialized();
        return pool.Get();
    }

    /// <summary>
    /// 從池中取得並初始化一個 Creature
    /// </summary>
    /// <param name="species">生物種族</param>
    /// <param name="attributes">生物屬性</param>
    /// <param name="position">生成位置</param>
    /// <param name="parent">父物件（可選）</param>
    public static Creature GetCreature(Species species, CreatureAttributes attributes, Vector3 position, Transform parent = null)
    {
        EnsureInitialized();
        
        Creature creature = pool.Get();
        
        // 設定位置和父物件
        if (parent != null)
        {
            creature.transform.SetParent(parent);
        }
        creature.transform.position = position;
        creature.transform.rotation = Quaternion.identity;
        
        // 初始化生物
        creature.Initialize(species, attributes, creature.gameObject);
        
        return creature;
    }

    /// <summary>
    /// 將 Creature 回收到池中
    /// </summary>
    public static void ReleaseCreature(Creature creature)
    {
        if (creature == null) return;
        
        pool.Release(creature);
    }

    /// <summary>
    /// 清空整個物件池
    /// </summary>
    public static void Clear()
    {
        pool.Clear();
        
        if (poolParent != null)
        {
            Object.Destroy(poolParent.gameObject);
            poolParent = null;
        }
        
        prefab = null;
    }

    /// <summary>
    /// 確保資源已初始化
    /// </summary>
    private static void EnsureInitialized()
    {
        if (prefab == null)
        {
            prefab = Resources.Load<GameObject>("Species/EmptyCreature");
            if (prefab == null)
            {
                Debug.LogError("CreaturePool: Failed to load EmptyCreature prefab");
            }
        }
        
        if (poolParent == null)
        {
            GameObject parentObj = new GameObject("[CreaturePool]");
            poolParent = parentObj.transform;
            Object.DontDestroyOnLoad(parentObj);
        }
    }

    // Factory method to create a new Creature instance
    private static Creature CreateCreature()
    {
        EnsureInitialized();
        
        if (prefab == null)
        {
            Debug.LogError("CreaturePool: Prefab is null, cannot create creature.");
            return null;
        }
        
        GameObject obj = Object.Instantiate(prefab, poolParent);
        obj.name = "PooledCreature";
        obj.SetActive(false);
        
        Creature creature = obj.GetComponent<Creature>();
        if (creature == null)
        {
            Debug.LogError("CreaturePool: Prefab does not have Creature component.");
            Object.Destroy(obj);
            return null;
        }
        
        return creature;
    }

    private static void ActionOnGet(Creature creature)
    {
        if (creature == null) return;
        
        // 啟用 GameObject
        creature.gameObject.SetActive(true);
        
        // 重置 Transform
        creature.transform.SetParent(null);
        creature.transform.localScale = Vector3.one;
    }

    private static void ActionOnRelease(Creature creature)
    {
        if (creature == null) return;
               
        // 停止所有協程
        creature.StopAllCoroutines();
        
        // 隱藏並移到池父物件下
        creature.gameObject.SetActive(false);
        creature.transform.SetParent(poolParent);
        creature.transform.position = Vector3.zero;
        creature.transform.rotation = Quaternion.identity;
    }

    private static void ActionOnDestroy(Creature creature)
    {
        if (creature != null && creature.gameObject != null)
        {
            Object.Destroy(creature.gameObject);
        }
    }
}
