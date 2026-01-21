/*
 * ===========================================================================================
 * EnvEntityManager - 環境實體管理器
 * ===========================================================================================
 * 
 * [功能說明]
 * 統一管理遊戲世界中所有環境實體（如草、肉、腐肉等）的生成、查詢和回收。
 * 負責維護實體的位置索引和物件池，提供高效的空間查詢功能。
 * 
 * [主要職責]
 * - 管理各類型實體的物件池（透過 EntityPool<T>）
 * - 維護實體的位置索引（透過 Dictionary<Vector2Int, GameObject>）
 * - 處理實體的生成邏輯（地形檢查、數量限制）
 * - 提供實體的查詢和移除功能
 * - 每個 Tick 自動生成環境實體（如草地上的草）
 * 
 * [與其他系統的關係]
 * - 由 Manager 創建和持有
 * - 實作 ITickable 介面，每 Tick 執行 OnTick
 * - 使用 EntityData 取得實體的詳細資料
 * - 使用 EntityPool<T> 管理實體的創建和回收
 * - 被 Perception.Items 用於查詢場景中的食物
 * 
 * -------------------------------------------------------------------------------------------
 * [公開屬性]
 * -------------------------------------------------------------------------------------------
 * 
 * ● EnvironmentEntities (Transform)
 *   - 說明：所有環境實體的父物件
 *   - 用途：組織場景階層、方便管理
 * 
 * -------------------------------------------------------------------------------------------
 * [公開方法]
 * -------------------------------------------------------------------------------------------
 * 
 * ● GetEntityCount(SpawnableEntityType type)
 *   - 說明：取得指定類型實體的當前數量
 *   - 回傳：該類型實體在場景中的數量
 *   - 用法：int grassCount = EnvEntityManager.GetEntityCount(SpawnableEntityType.Grass);
 * 
 * ● GetPool(SpawnableEntityType type)
 *   - 說明：取得指定類型的物件池
 *   - 回傳：IEntityPool 介面，可查詢池狀態
 *   - 用法：var pool = EnvEntityManager.GetPool(SpawnableEntityType.Meat);
 * 
 * ● SpawnEntity(SpawnableEntityType type, Vector2Int pos)
 *   - 說明：在指定位置生成實體
 *   - 參數：
 *       type - 實體類型
 *       pos  - 生成位置（格座標）
 *   - 回傳：是否成功生成（位置已被佔用或地形不符會失敗）
 *   - 用法：bool success = EnvEntityManager.SpawnEntity(SpawnableEntityType.Meat, new Vector2Int(10, 20));
 * 
 * ● RemoveEntity(SpawnableEntityType type, Vector2Int pos)
 *   - 說明：移除指定位置的實體並回收到物件池
 *   - 回傳：是否成功移除
 *   - 用法：EnvEntityManager.RemoveEntity(SpawnableEntityType.Grass, grassPosition);
 * 
 * ● GetEntity<T>(SpawnableEntityType type, Vector2Int pos)
 *   - 說明：取得指定位置的實體
 *   - 回傳：該位置的實體，若不存在則回傳 null
 *   - 用法：Grass grass = EnvEntityManager.GetEntity<Grass>(SpawnableEntityType.Grass, pos);
 * 
 * ● ClearAll()
 *   - 說明：清空所有實體和物件池
 *   - 用法：EnvEntityManager.ClearAll();
 * 
 * -------------------------------------------------------------------------------------------
 * [內部運作]
 * -------------------------------------------------------------------------------------------
 * 
 * 初始化流程：
 * 1. 創建 EnvironmentEntities 父物件
 * 2. 遍歷所有 SpawnableEntityType
 * 3. 為每種類型創建位置字典和物件池
 * 4. 使用反射創建正確的泛型 EntityPool<T>
 * 
 * 每 Tick 執行：
 * 1. 呼叫 RandomlySpawnEntity 嘗試生成草
 * 2. 檢查數量限制
 * 3. 隨機選擇位置並檢查地形
 * 4. 在合適的位置生成實體
 * 
 * ===========================================================================================
 */

using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// 環境實體管理器 - 統一管理所有環境實體的生成、查詢和回收
/// </summary>
public class EnvEntityManager : ITickable
{
    // ========== 測試用常數（暫時性） ==========
    /// <summary>生成範圍最小值（包含）</summary>
    private const int MINSPAWNRANGE = 0;
    /// <summary>生成範圍最大值（包含）</summary>
    private const int MAXSPAWNRANGE = 500;

    // ========== 公開屬性 ==========
    
    /// <summary>所有環境實體的父物件</summary>
    public Transform EnvironmentEntities { get; private set; }

    // ========== 私有欄位 ==========
    
    /// <summary>
    /// 實體位置索引字典
    /// Key: 實體類型
    /// Value: 該類型所有實體的位置對應表（位置 → GameObject）
    /// </summary>
    private readonly Dictionary<EntityData.SpawnableEntityType, Dictionary<Vector2Int, GameObject>> _entityDict = new();
    
    /// <summary>
    /// 實體物件池字典
    /// Key: 實體類型
    /// Value: 該類型的物件池（IEntityPool 介面）
    /// </summary>
    private readonly Dictionary<EntityData.SpawnableEntityType, IEntityPool> _entityPool = new();

    // ========== 建構子 ==========
    
    /// <summary>
    /// 建構子 - 初始化環境實體管理器
    /// </summary>
    public EnvEntityManager()
    {
        Initialize();
    }

    /// <summary>
    /// 初始化 - 創建父物件和所有類型的物件池
    /// </summary>
    private void Initialize()
    {
        // 創建環境實體的父物件
        GameObject env_entites_prefab = Resources.Load<GameObject>("Prefabs/Parents/EnvironmentEntities");
        EnvironmentEntities = UnityEngine.Object.Instantiate(env_entites_prefab).transform;

        // 為每種實體類型初始化字典和物件池
        foreach (EntityData.SpawnableEntityType type in Enum.GetValues(typeof(EntityData.SpawnableEntityType)))
        {
            SpawnableEntity entityData = EntityData.GetSpawnableEntity(type);
            
            // 創建該類型的位置索引字典
            _entityDict.Add(type, new Dictionary<Vector2Int, GameObject>());

            // 使用反射創建正確的泛型物件池
            // 例如：EntityPool<Grass>、EntityPool<Meat>
            Type poolType = typeof(EntityPool<>).MakeGenericType(entityData.ClassType);
            
            // 注意：必須使用 object[] 包裝參數，否則 enum 會被轉成 int 導致找不到建構子
            IEntityPool poolInstance = (IEntityPool)Activator.CreateInstance(
                poolType, 
                new object[] { type, 1000 }
            );
            _entityPool.Add(type, poolInstance);
        }
    }

    // ========== ITickable 實作 ==========
    
    /// <summary>
    /// 啟用時向 TickManager 註冊
    /// </summary>
    public void OnEnable()
    {
        TickManager.Instance.RegisterTickable(OnTick);
    }

    /// <summary>
    /// 停用時從 TickManager 取消註冊
    /// </summary>
    public void OnDisable()
    {
        TickManager.Instance.UnregisterTickable(OnTick);
    }

    /// <summary>
    /// 每個 Tick 執行 - 處理環境實體的自動生成
    /// </summary>
    public void OnTick()
    {
        // 每個 Tick 嘗試生成草
        RandomlySpawnEntity(EntityData.SpawnableEntityType.Grass);
    }

    // ========== 公開方法 ==========
    
    /// <summary>
    /// 取得指定類型實體的當前數量
    /// </summary>
    /// <param name="type">實體類型</param>
    /// <returns>該類型實體在場景中的數量</returns>
    public int GetEntityCount(EntityData.SpawnableEntityType type)
    {
        if (_entityDict.TryGetValue(type, out var dict))
        {
            return dict.Count;
        }
        return 0;
    }

    /// <summary>
    /// 取得指定類型的物件池
    /// </summary>
    /// <param name="type">實體類型</param>
    /// <returns>IEntityPool 介面</returns>
    public IEntityPool GetPool(EntityData.SpawnableEntityType type)
    {
        if (_entityPool.TryGetValue(type, out var pool))
        {
            return pool;
        }
        return null;
    }

    /// <summary>
    /// 在指定位置生成實體
    /// </summary>
    /// <param name="spawnableType">實體類型</param>
    /// <param name="pos">生成位置（格座標）</param>
    /// <returns>是否成功生成</returns>
    public bool SpawnEntity(EntityData.SpawnableEntityType spawnableType, Vector2Int pos)
    {
        // 檢查位置是否已被佔用
        if (_entityDict[spawnableType].ContainsKey(pos)) return false;

        SpawnableEntity entityData = EntityData.GetSpawnableEntity(spawnableType);

        // 檢查地形是否允許生成
        if (entityData.SpawnableTerrain != null && entityData.SpawnableTerrain.Count > 0)
        {
            TerrainType currentTerrain = TerrainGenerator.Instance.GetDefinitionMap().GetTerrain(pos);
            bool canSpawn = false;
            
            foreach (var terrainType in entityData.SpawnableTerrain)
            {
                if (currentTerrain == terrainType)
                {
                    canSpawn = true;
                    break;
                }
            }
            
            if (!canSpawn) return false;
        }

        // 從物件池取得實體
        MonoBehaviour entity = _entityPool[spawnableType].GetEntity(pos, EnvironmentEntities);
        if (entity == null) return false;

        // 記錄到位置索引字典中
        _entityDict[spawnableType].Add(pos, entity.gameObject);

        // 如果實體是 Edible，呼叫其初始化方法
        if (entity is Edible edible)
        {
            edible.Initialize();
        }

        return true;
    }

    /// <summary>
    /// 移除指定位置的實體並回收到物件池
    /// </summary>
    /// <param name="spawnableType">實體類型</param>
    /// <param name="pos">實體位置</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveEntity(EntityData.SpawnableEntityType spawnableType, Vector2Int pos)
    {
        // 檢查該位置是否有實體
        if (!_entityDict[spawnableType].TryGetValue(pos, out GameObject entityObj))
        {
            return false;
        }

        // 從位置索引中移除
        _entityDict[spawnableType].Remove(pos);

        // 回收到物件池
        MonoBehaviour entity = entityObj.GetComponent<MonoBehaviour>();
        if (entity != null)
        {
            _entityPool[spawnableType].ReleaseEntity(entity);
        }

        return true;
    }

    /// <summary>
    /// 取得指定位置的實體
    /// </summary>
    /// <typeparam name="T">實體的 MonoBehaviour 類型</typeparam>
    /// <param name="spawnableType">實體類型</param>
    /// <param name="position">查詢位置</param>
    /// <returns>該位置的實體，若不存在則回傳 null</returns>
    public T GetEntity<T>(EntityData.SpawnableEntityType spawnableType, Vector2Int position) where T : MonoBehaviour
    {
        if (_entityDict.TryGetValue(spawnableType, out var entityPositions))
        {
            if (entityPositions.TryGetValue(position, out var entityObj))
            {
                return entityObj.GetComponent<T>();
            }
        }
        return null;
    }

    /// <summary>
    /// 清空所有實體和物件池
    /// </summary>
    public void ClearAll()
    {
        foreach (var type in _entityDict.Keys)
        {
            _entityDict[type].Clear();
            _entityPool[type].ClearPool();
        }
    }

    // ========== 私有方法 ==========
    
    /// <summary>
    /// 隨機生成指定類型的實體
    /// </summary>
    /// <param name="spawnableType">要生成的實體類型</param>
    private void RandomlySpawnEntity(EntityData.SpawnableEntityType spawnableType)
    {
        SpawnableEntity entityData = EntityData.GetSpawnableEntity(spawnableType);
        
        // 檢查是否達到最大數量限制
        if (GetEntityCount(spawnableType) >= entityData.MaxSpawnableValue) return;

        // 隨機選擇一個起始位置
        Vector2Int position = new Vector2Int(
            Random.Range(MINSPAWNRANGE, MAXSPAWNRANGE + 1),
            Random.Range(MINSPAWNRANGE, MAXSPAWNRANGE + 1)
        );

        // 檢查起始位置是否已被佔用
        if (_entityDict[spawnableType].ContainsKey(position)) return;

        // 在起始位置周圍隨機取得多個位置
        var randomPositions = GetRandomPosition(position, 3, Random.Range(1, 5));
        
        // 嘗試在每個位置生成實體
        foreach (var pos in randomPositions)
        {
            SpawnEntity(spawnableType, pos);
        }
    }

    /// <summary>
    /// 在指定位置周圍隨機取得多個不重複的位置
    /// </summary>
    /// <param name="position">中心位置</param>
    /// <param name="r">最大半徑</param>
    /// <param name="n">需要的位置數量</param>
    /// <returns>隨機位置列表</returns>
    private List<Vector2Int> GetRandomPosition(Vector2Int position, int r, int n)
    {
        List<Vector2Int> positions = new();
        HashSet<Vector2Int> usedPositions = new();
        uint tries = 0;
        
        // 嘗試取得 n 個不重複的位置
        while (positions.Count < n && tries < n * 10)
        {
            // 隨機角度和半徑
            float angle = Random.Range(0f, 360f);
            float radius = Random.Range(0f, r);
            
            // 計算新位置
            Vector2Int newPosition = position + new Vector2Int(
                Mathf.RoundToInt(radius * Mathf.Cos(angle * Mathf.Deg2Rad)),
                Mathf.RoundToInt(radius * Mathf.Sin(angle * Mathf.Deg2Rad))
            );
            
            // 檢查是否已使用過
            if (!usedPositions.Contains(newPosition))
            {
                usedPositions.Add(newPosition);
                positions.Add(newPosition);
            }
            tries++;
        }
        
        return positions;
    }
}
