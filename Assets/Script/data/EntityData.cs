/*
 * ===========================================================================================
 * EntityData - 可生成實體的資料定義
 * ===========================================================================================
 * 
 * [功能說明]
 * 集中管理所有可在環境中生成的實體類型資料，包括：
 * - 實體類型的定義（SpawnableEntityType）
 * - 各類型實體的詳細資料（SpawnableEntity）
 * - FoodType 與 SpawnableEntityType 之間的對應關係
 * 
 * [設計目的]
 * - 提供統一的資料來源，避免硬編碼散落各處
 * - 支援類型安全的實體查詢和轉換
 * - 在編譯時期驗證資料完整性
 * 
 * -------------------------------------------------------------------------------------------
 * [SpawnableEntityType 列舉]
 * -------------------------------------------------------------------------------------------
 * 定義所有可生成的實體類型：
 * - Grass   : 草（植物性食物）
 * - Meat    : 肉（動物性食物，生物死亡時產生）
 * - Carrion : 腐肉（肉類腐爛後產生）
 * 
 * -------------------------------------------------------------------------------------------
 * [公開方法]
 * -------------------------------------------------------------------------------------------
 * 
 * ● FoodType2SpawnableType(FoodType foodType)
 *   - 說明：將 FoodType 轉換為對應的 SpawnableEntityType
 *   - 回傳：對應的 SpawnableEntityType，若無對應則回傳 null
 *   - 用法：var type = EntityData.FoodType2SpawnableType(FoodType.Grass);
 * 
 * ● SpawnableType2FoodType(SpawnableEntityType spawnableType)
 *   - 說明：將 SpawnableEntityType 轉換為對應的 FoodType
 *   - 回傳：對應的 FoodType，若無對應則回傳 null
 *   - 用法：var food = EntityData.SpawnableType2FoodType(SpawnableEntityType.Meat);
 * 
 * ● GetSpawnableEntity(SpawnableEntityType spawnableType)
 *   - 說明：取得指定類型的實體資料
 *   - 回傳：SpawnableEntity 物件，包含該類型的所有詳細資料
 *   - 用法：var data = EntityData.GetSpawnableEntity(SpawnableEntityType.Grass);
 * 
 * -------------------------------------------------------------------------------------------
 * [SpawnableEntity 類別]
 * -------------------------------------------------------------------------------------------
 * 儲存單一實體類型的詳細資料：
 * 
 * ● ClassType (Type)
 *   - 實體的 C# 類別類型（如 typeof(Grass)）
 *   - 用於反射創建對應的 EntityPool<T>
 * 
 * ● PrefabPath (string)
 *   - Prefab 在 Resources 資料夾中的路徑
 *   - 用於載入實體的預製物件
 * 
 * ● MaxSpawnableValue (int)
 *   - 該類型實體的最大數量限制
 *   - 用於控制世界中同時存在的實體數量
 * 
 * ● FoodType (FoodType?)
 *   - 對應的食物類型（可為 null）
 *   - 用於與生物的進食系統連接
 * 
 * ● SpawnableTerrain (List<TerrainType>)
 *   - 可生成此實體的地形類型列表
 *   - 若為 null 則表示可在任何地形生成
 * 
 * -------------------------------------------------------------------------------------------
 * [新增實體類型步驟]
 * -------------------------------------------------------------------------------------------
 * 1. 在 SpawnableEntityType 列舉中新增類型
 * 2. 在 spawnableDict 中新增對應的 SpawnableEntity
 * 3. 若該實體也是食物，在 foodTypeMapping 中新增對應關係
 * 4. 創建對應的實體類別（繼承 Edible 或其他基類）
 * 5. 創建對應的 Prefab 並放置於正確的 Resources 路徑
 * 
 * ===========================================================================================
 */

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 實體資料管理類別 - 集中管理所有可生成實體的定義資料
/// </summary>
public static class EntityData
{
    /// <summary>
    /// 可生成的實體類型列舉
    /// </summary>
    public enum SpawnableEntityType
    {
        Grass,      // 草（植物性食物）
        Meat,       // 肉（動物性食物）
        Carrion     // 腐肉
    }

    // ========== 實體資料字典 ==========
    // 儲存每種實體類型的詳細資料
    private static readonly Dictionary<SpawnableEntityType, SpawnableEntity> spawnableDict = new()
    {
        // 草：最大 5000 個，只能在草地地形生成
        { SpawnableEntityType.Grass, new SpawnableEntity(
            typeof(Grass), 
            "Prefabs/Edible/Grass", 
            5000, 
            FoodType.Grass, 
            new List<TerrainType> { TerrainType.Grass }) 
        },
        // 肉：無數量限制，可在任何地形生成
        { SpawnableEntityType.Meat, new SpawnableEntity(
            typeof(Meat), 
            "Prefabs/Edible/Meat", 
            int.MaxValue, 
            FoodType.Meat) 
        },
        // 腐肉：無數量限制，可在任何地形生成
        { SpawnableEntityType.Carrion, new SpawnableEntity(
            typeof(Carrion), 
            "Prefabs/Edible/Carrion", 
            int.MaxValue, 
            FoodType.Carrion) 
        }
    };

    // ========== FoodType 對應表 ==========
    // 用於快速查詢 FoodType 對應的 SpawnableEntityType
    private static readonly Dictionary<FoodType, SpawnableEntityType> foodTypeMapping = new()
    {
        { FoodType.Grass, SpawnableEntityType.Grass },
        { FoodType.Meat, SpawnableEntityType.Meat },
        { FoodType.Carrion, SpawnableEntityType.Carrion }
    };

    /// <summary>
    /// 將 FoodType 轉換為對應的 SpawnableEntityType
    /// </summary>
    /// <param name="foodType">食物類型</param>
    /// <returns>對應的 SpawnableEntityType，若無對應則回傳 null</returns>
    public static SpawnableEntityType? FoodType2SpawnableType(FoodType foodType)
    {
        if (foodTypeMapping.TryGetValue(foodType, out var spawnableType))
        {
            return spawnableType;
        }
        return null;
    }

    /// <summary>
    /// 將 SpawnableEntityType 轉換為對應的 FoodType
    /// </summary>
    /// <param name="spawnableType">可生成實體類型</param>
    /// <returns>對應的 FoodType，若無對應則回傳 null</returns>
    public static FoodType? SpawnableType2FoodType(SpawnableEntityType spawnableType)
    {
        if (spawnableDict.TryGetValue(spawnableType, out var entity))
        {
            return entity.FoodType;
        }
        return null;
    }

    /// <summary>
    /// 取得指定類型的實體資料
    /// </summary>
    /// <param name="spawnableType">可生成實體類型</param>
    /// <returns>SpawnableEntity 資料物件</returns>
    public static SpawnableEntity GetSpawnableEntity(SpawnableEntityType spawnableType)
    {
        if (spawnableDict.TryGetValue(spawnableType, out var entity))
        {
            return entity;
        }
        return null;
    }

    /// <summary>
    /// 靜態建構子 - 驗證資料完整性
    /// </summary>
    static EntityData()
    {
        // 檢查 spawnableDict 是否包含所有 SpawnableEntityType
        if (spawnableDict.Count != Enum.GetValues(typeof(SpawnableEntityType)).Length)
        {
            Debug.LogError("[EntityData] spawnableDict 的數量與 SpawnableEntityType 列舉不符！");
            foreach (SpawnableEntityType type in Enum.GetValues(typeof(SpawnableEntityType)))
            {
                if (!spawnableDict.ContainsKey(type))
                {
                    Debug.LogError($"[EntityData] SpawnableEntityType.{type} 未在 spawnableDict 中定義！");
                }
            }
        }
    }
}

/// <summary>
/// 可生成實體資料類別 - 儲存單一實體類型的詳細資料
/// </summary>
public class SpawnableEntity
{
    /// <summary>實體的 C# 類別類型（用於反射創建 EntityPool）</summary>
    public Type ClassType { get; private set; }
    
    /// <summary>Prefab 在 Resources 資料夾中的路徑</summary>
    public string PrefabPath { get; }
    
    /// <summary>可生成此實體的地形類型列表（null 表示可在任何地形生成）</summary>
    public List<TerrainType> SpawnableTerrain { get; private set; }
    
    /// <summary>該類型實體的最大數量限制</summary>
    public int MaxSpawnableValue { get; private set; }
    
    /// <summary>對應的食物類型（可為 null）</summary>
    public FoodType? FoodType { get; private set; }

    /// <summary>
    /// 建構子
    /// </summary>
    /// <param name="type">實體的 C# 類別類型</param>
    /// <param name="prefabPath">Prefab 路徑</param>
    /// <param name="maxVal">最大數量限制</param>
    /// <param name="foodType">對應的食物類型（可選）</param>
    /// <param name="spawnableTerrain">可生成的地形列表（可選）</param>
    public SpawnableEntity(Type type, string prefabPath, int maxVal, FoodType? foodType = null, List<TerrainType> spawnableTerrain = null)
    {
        ClassType = type;
        PrefabPath = prefabPath;
        MaxSpawnableValue = maxVal;
        FoodType = foodType;
        SpawnableTerrain = spawnableTerrain;
    }
}

