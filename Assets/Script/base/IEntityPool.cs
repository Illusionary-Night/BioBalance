/*
 * ===========================================================================================
 * IEntityPool - 實體物件池介面
 * ===========================================================================================
 * 
 * [功能說明]
 * 定義實體物件池的標準介面，用於抽象化不同類型的 EntityPool<T>。
 * 由於 EntityPool<T> 是泛型類別，無法直接用統一的方式儲存和操作，
 * 因此透過這個介面來提供非泛型的存取方式。
 * 
 * [設計目的]
 * - 允許 EnvEntityManager 使用 Dictionary<SpawnableEntityType, IEntityPool> 統一管理所有類型的池
 * - 透過反射創建的泛型池可以轉型為此介面進行操作
 * - 提供基本的池狀態查詢和實體管理功能
 * 
 * -------------------------------------------------------------------------------------------
 * [公開屬性]
 * -------------------------------------------------------------------------------------------
 * 
 * ● CountActive (int)
 *   - 說明：目前正在場景中使用的實體數量
 * 
 * ● CountInactive (int)
 *   - 說明：目前在池中閒置、等待被重用的實體數量
 * 
 * ● CountAll (int)
 *   - 說明：物件池管理的實體總數（活躍 + 閒置）
 * 
 * -------------------------------------------------------------------------------------------
 * [公開方法]
 * -------------------------------------------------------------------------------------------
 * 
 * ● GetEntity()
 *   - 說明：從池中取得一個實體（未設定位置）
 *   - 回傳：MonoBehaviour 類型的實體
 * 
 * ● GetEntity(Vector2Int position, Transform parent = null)
 *   - 說明：從池中取得一個實體，並設定位置和父物件
 *   - 參數：
 *       position - 實體的生成位置（格座標）
 *       parent   - 父物件（可選）
 *   - 回傳：MonoBehaviour 類型的實體
 * 
 * ● ReleaseEntity(MonoBehaviour entity)
 *   - 說明：將實體回收到池中
 *   - 參數：entity - 要回收的實體
 * 
 * ● ClearPool()
 *   - 說明：清空整個物件池，銷毀所有實體
 * 
 * -------------------------------------------------------------------------------------------
 * [使用範例]
 * -------------------------------------------------------------------------------------------
 * 
 * // 在 EnvEntityManager 中使用
 * IEntityPool pool = _entityPool[SpawnableEntityType.Grass];
 * MonoBehaviour entity = pool.GetEntity(new Vector2Int(10, 20), parentTransform);
 * 
 * // 回收實體
 * pool.ReleaseEntity(entity);
 * 
 * // 查詢池狀態
 * Debug.Log($"活躍: {pool.CountActive}, 閒置: {pool.CountInactive}");
 * 
 * ===========================================================================================
 */

using UnityEngine;

/// <summary>
/// 實體物件池介面 - 提供非泛型的池操作方法
/// </summary>
public interface IEntityPool
{
    /// <summary>目前正在使用中的實體數量</summary>
    int CountActive { get; }
    
    /// <summary>目前在池中閒置的實體數量</summary>
    int CountInactive { get; }
    
    /// <summary>池管理的實體總數（活躍 + 閒置）</summary>
    int CountAll { get; }

    /// <summary>
    /// 從池中取得一個實體
    /// </summary>
    /// <returns>實體的 MonoBehaviour 組件</returns>
    MonoBehaviour GetEntity();
    
    /// <summary>
    /// 從池中取得一個實體，並設定位置和父物件
    /// </summary>
    /// <param name="position">生成位置（格座標）</param>
    /// <param name="parent">父物件（可選）</param>
    /// <returns>實體的 MonoBehaviour 組件</returns>
    MonoBehaviour GetEntity(Vector2Int position, Transform parent = null);
    
    /// <summary>
    /// 將實體回收到池中
    /// </summary>
    /// <param name="entity">要回收的實體</param>
    void ReleaseEntity(MonoBehaviour entity);
    
    /// <summary>
    /// 清空物件池，銷毀所有實體
    /// </summary>
    void ClearPool();
}