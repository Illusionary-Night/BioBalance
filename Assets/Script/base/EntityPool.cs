/*
 * ===========================================================================================
 * EntityPool<T> - 泛型實體物件池
 * ===========================================================================================
 * 
 * [功能說明]
 * 使用 Unity 的 ObjectPool 來管理特定類型實體的生成與回收，
 * 避免頻繁的 Instantiate/Destroy 造成的效能問題與 GC 壓力。
 * 
 * [設計特點]
 * - 泛型設計：每種實體類型（Grass、Meat 等）都有獨立的池
 * - 實作 IEntityPool 介面：允許透過非泛型方式操作池
 * - 自動管理 Prefab 載入和父物件創建
 * - 提供完整的生命週期回調（創建、取得、釋放、銷毀）
 * 
 * -------------------------------------------------------------------------------------------
 * [公開屬性]
 * -------------------------------------------------------------------------------------------
 * 
 * ● CountActive (int)
 *   - 說明：目前正在場景中使用的實體數量
 * 
 * ● CountInactive (int)
 *   - 說明：目前在池中閒置的實體數量
 * 
 * ● CountAll (int)
 *   - 說明：池管理的實體總數（活躍 + 閒置）
 * 
 * -------------------------------------------------------------------------------------------
 * [公開方法（泛型版本）]
 * -------------------------------------------------------------------------------------------
 * 
 * ● GetEntityTyped()
 *   - 說明：從池中取得一個具體類型的實體
 *   - 回傳：T 類型的實體
 *   - 用法：Grass grass = grassPool.GetEntityTyped();
 * 
 * ● GetEntityTyped(Vector2Int position, Transform parent = null)
 *   - 說明：從池中取得實體，並設定位置和父物件
 *   - 參數：
 *       position - 生成位置（格座標）
 *       parent   - 父物件（可選）
 *   - 回傳：T 類型的實體
 * 
 * ● ReleaseEntityTyped(T entity)
 *   - 說明：將具體類型的實體回收到池中
 *   - 參數：entity - 要回收的實體
 * 
 * ● ClearPool()
 *   - 說明：清空物件池，銷毀所有實體
 * 
 * -------------------------------------------------------------------------------------------
 * [IEntityPool 介面實作]
 * -------------------------------------------------------------------------------------------
 * 提供非泛型版本的方法，供 EnvEntityManager 透過介面操作：
 * - GetEntity() / GetEntity(position, parent)
 * - ReleaseEntity(entity)
 * 
 * -------------------------------------------------------------------------------------------
 * [內部運作流程]
 * -------------------------------------------------------------------------------------------
 * 
 * 創建實體 (CreateEntity)：
 * 1. 確保 Prefab 已載入
 * 2. 使用 Instantiate 創建實例
 * 3. 設為非啟用狀態
 * 4. 放置於池父物件下
 * 
 * 取得實體 (ActionOnGet)：
 * 1. 啟用 GameObject
 * 2. 從池父物件脫離
 * 3. 重置縮放
 * 
 * 釋放實體 (ActionOnRelease)：
 * 1. 停止所有協程
 * 2. 停用 GameObject
 * 3. 移回池父物件下
 * 4. 重置位置和旋轉
 * 
 * ===========================================================================================
 */

using UnityEngine;
using UnityEngine.Pool;
using System;

/// <summary>
/// 泛型實體物件池 - 管理特定類型實體的生成與回收
/// </summary>
/// <typeparam name="T">實體的 MonoBehaviour 類型（如 Grass、Meat）</typeparam>
public class EntityPool<T> : IEntityPool where T : MonoBehaviour
{
    // ========== 私有欄位 ==========
    
    /// <summary>此實體類型的資料</summary>
    private readonly SpawnableEntity _entityData;
    
    /// <summary>載入的 Prefab</summary>
    private GameObject _prefab;
    
    /// <summary>池的父物件（用於組織閒置的實體）</summary>
    private Transform _poolParent;

    /// <summary>Unity 內建的物件池</summary>
    private ObjectPool<T> pool;

    // ========== 公開屬性 ==========
    
    /// <summary>目前正在使用中的實體數量</summary>
    public int CountActive => pool.CountActive;
    
    /// <summary>目前在池中閒置的實體數量</summary>
    public int CountInactive => pool.CountInactive;
    
    /// <summary>池管理的實體總數</summary>
    public int CountAll => pool.CountAll;

    // ========== 建構子 ==========
    
    /// <summary>
    /// 建構子 - 創建指定類型的實體池
    /// </summary>
    /// <param name="spawnableType">實體類型</param>
    /// <param name="defaultCapacity">預設容量（預設 1000）</param>
    public EntityPool(EntityData.SpawnableEntityType spawnableType, int defaultCapacity = 1000)
    {
        // 取得此類型的實體資料
        _entityData = EntityData.GetSpawnableEntity(spawnableType);

        // 確保資源已初始化
        EnsureInitialized();

        // 創建 Unity 內建的物件池
        pool = new ObjectPool<T>(
            createFunc: CreateEntity,           // 創建新實體時呼叫
            actionOnGet: ActionOnGet,           // 從池中取得實體時呼叫
            actionOnRelease: ActionOnRelease,   // 將實體放回池中時呼叫
            actionOnDestroy: ActionOnDestroy,   // 銷毀實體時呼叫
            collectionCheck: false,             // 關閉重複回收檢查（效能考量）
            defaultCapacity: defaultCapacity,   // 預設容量
            maxSize: Math.Min(_entityData.MaxSpawnableValue, 100_000)  // 最大容量
        );
    }

    // ========== 初始化 ==========
    
    /// <summary>
    /// 確保 Prefab 和父物件已初始化
    /// </summary>
    private void EnsureInitialized()
    {
        // 載入 Prefab
        if (_prefab == null)
        {
            _prefab = Resources.Load<GameObject>(_entityData.PrefabPath);
            if (_prefab == null)
            {
                Debug.LogError($"[EntityPool<{typeof(T).Name}>] 無法從 {_entityData.PrefabPath} 載入 Prefab");
            }
        }

        // 創建池父物件
        if (_poolParent == null)
        {
            GameObject parentObj = new GameObject($"[{typeof(T).Name}Pool]");
            _poolParent = parentObj.transform;
            UnityEngine.Object.DontDestroyOnLoad(parentObj);  // 場景切換時不銷毀
        }
    }

    // ========== 泛型公開方法 ==========
    
    /// <summary>
    /// 從池中取得一個實體（泛型版本）
    /// </summary>
    /// <returns>T 類型的實體</returns>
    public T GetEntityTyped()
    {
        EnsureInitialized();
        return pool.Get();
    }

    /// <summary>
    /// 從池中取得一個實體，並設定位置和父物件（泛型版本）
    /// </summary>
    /// <param name="position">生成位置（格座標）</param>
    /// <param name="parent">父物件（可選）</param>
    /// <returns>T 類型的實體</returns>
    public T GetEntityTyped(Vector2Int position, Transform parent = null)
    {
        EnsureInitialized();

        T entity = GetEntityTyped();
        if (parent != null) entity.transform.SetParent(parent);
        entity.transform.position = new Vector3(position.x, position.y, 0);
        return entity;
    }

    /// <summary>
    /// 將實體回收到池中（泛型版本）
    /// </summary>
    /// <param name="entity">要回收的實體</param>
    public void ReleaseEntityTyped(T entity)
    {
        if (entity != null) pool.Release(entity);
    }

    // ========== IEntityPool 介面實作 ==========
    
    /// <summary>
    /// 從池中取得一個實體（介面版本）
    /// </summary>
    MonoBehaviour IEntityPool.GetEntity()
    {
        return GetEntityTyped();
    }

    /// <summary>
    /// 從池中取得一個實體，並設定位置和父物件（介面版本）
    /// </summary>
    MonoBehaviour IEntityPool.GetEntity(Vector2Int position, Transform parent)
    {
        return GetEntityTyped(position, parent);
    }

    /// <summary>
    /// 將實體回收到池中（介面版本）
    /// </summary>
    void IEntityPool.ReleaseEntity(MonoBehaviour entity)
    {
        // 嘗試轉型為正確的類型
        if (entity is T typedEntity)
        {
            ReleaseEntityTyped(typedEntity);
        }
        else if (entity != null)
        {
            Debug.LogWarning($"[EntityPool<{typeof(T).Name}>] 無法回收類型為 {entity.GetType().Name} 的實體");
        }
    }

    /// <summary>
    /// 清空物件池，銷毀所有實體
    /// </summary>
    public void ClearPool()
    {
        pool.Clear();
        
        // 銷毀池父物件
        if (_poolParent != null)
        {
            UnityEngine.Object.Destroy(_poolParent.gameObject);
            _poolParent = null;
        }

        _prefab = null;
    }

    // ========== ObjectPool 回調方法 ==========
    
    /// <summary>
    /// 創建新實體時呼叫
    /// </summary>
    private T CreateEntity()
    {
        EnsureInitialized();

        if (_prefab == null)
        {
            Debug.LogError($"[EntityPool<{typeof(T).Name}>] Prefab 為 null，無法創建實體");
            return null;
        }

        // 實例化 Prefab
        GameObject obj = UnityEngine.Object.Instantiate(_prefab, _poolParent);
        obj.name = "Pooled" + _prefab.name;
        obj.SetActive(false);  // 創建時保持停用

        // 取得組件
        T entity = obj.GetComponent<T>();
        if (entity == null)
        {
            Debug.LogError($"[EntityPool<{typeof(T).Name}>] Prefab 上沒有 {typeof(T).Name} 組件");
            UnityEngine.Object.Destroy(obj);
            return null;
        }
        return entity;
    }

    /// <summary>
    /// 從池中取得實體時呼叫
    /// </summary>
    private void ActionOnGet(T entity)
    {
        if (entity == null) return;

        entity.gameObject.SetActive(true);          // 啟用 GameObject
        entity.transform.SetParent(null);           // 脫離池父物件
        entity.transform.localScale = Vector3.one;  // 重置縮放
    }

    /// <summary>
    /// 將實體放回池中時呼叫
    /// </summary>
    private void ActionOnRelease(T entity)
    {
        if (entity == null) return;

        entity.StopAllCoroutines();  // 停止所有協程

        entity.gameObject.SetActive(false);             // 停用 GameObject
        entity.transform.SetParent(_poolParent);        // 移回池父物件下
        entity.transform.position = Vector3.zero;       // 重置位置
        entity.transform.rotation = Quaternion.identity; // 重置旋轉
    }

    /// <summary>
    /// 銷毀實體時呼叫（池達到最大容量或清空時）
    /// </summary>
    private void ActionOnDestroy(T entity)
    {
        if (entity != null && entity.gameObject != null)
        {
            UnityEngine.Object.Destroy(entity.gameObject);
        }
    }
}
