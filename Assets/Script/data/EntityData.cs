using System;
using System.Collections.Generic;
using UnityEngine;

// This static class holds data related to various entities that can be spawned in the environment
public static class EntityData
{
    public enum SpawnableEntityType
    {
        Grass,
        Meat,
        Carrion
    }

    private static readonly Dictionary<SpawnableEntityType, SpawnableEntity> spawnableDict = new()
    {
        { SpawnableEntityType.Grass, new SpawnableEntity(typeof(Grass), "Prefabs/Edible/Grass", 5000, FoodType.Grass, new List<TerrainType> { TerrainType.Grass }) },
        { SpawnableEntityType.Meat, new SpawnableEntity(typeof(Meat), "Prefabs/Edible/Meat", int.MaxValue, FoodType.Meat) },
        { SpawnableEntityType.Carrion, new SpawnableEntity(typeof(Carrion), "Prefabs/Edible/Carrion", int.MaxValue, FoodType.Carrion) }
    };

    private static readonly Dictionary<FoodType, SpawnableEntityType> foodTypeMapping = new()
    {
        { FoodType.Grass, SpawnableEntityType.Grass },
        { FoodType.Meat, SpawnableEntityType.Meat },
        { FoodType.Carrion, SpawnableEntityType.Carrion }
    };

    public static SpawnableEntityType? FoodType2SpawnableType(FoodType foodType)
    {
        if (foodTypeMapping.TryGetValue(foodType, out var spawnableType))
        {
            return spawnableType;
        }
        return null;
    }

    public static FoodType? SpawnableType2FoodType(SpawnableEntityType spawnableType)
    {
        if (spawnableDict.TryGetValue(spawnableType, out var entity))
        {
            return entity.FoodType;
        }
        return null;
    }

    public static SpawnableEntity GetSpawnableEntity(SpawnableEntityType spawnableType)
    {
        if (spawnableDict.TryGetValue(spawnableType, out var entity))
        {
            return entity;
        }
        return null;
    }

    // Chech the Dictionary and the enum match
    static EntityData()
    {
        if (spawnableDict.Count != System.Enum.GetValues(typeof(SpawnableEntityType)).Length)
        {
            Debug.LogError("spawnableDict count does not match SpawnableEntityType enum count.");
            foreach (SpawnableEntityType type in System.Enum.GetValues(typeof(SpawnableEntityType)))
            {
                if (!spawnableDict.ContainsKey(type))
                {
                    Debug.LogError($"SpawnableEntityType {type} is not defined in spawnableDict.");
                }
            }
        }
    }
}

// This class is used for safe-keeping data related to spawnable entities
// Represents an entity that can be spawned in the environment
public class SpawnableEntity
{
    public Type ClassType { get; private set; }
    // The path to the prefab resource
    public string PrefabPath { get; }
    // Indicates whether the entity can be spawned
    public List<TerrainType> SpawnableTerrain { get; private set; }
    public int MaxSpawnableValue { get; private set; }
    public FoodType? FoodType { get; private set; }

    public SpawnableEntity(Type type, string prefabPath, int maxVal, FoodType? foodType = null, List<TerrainType> spawnableTerrain = null)
    {
        ClassType = type;
        PrefabPath = prefabPath;
        MaxSpawnableValue = maxVal;
        FoodType = foodType;
        // If no specific terrain is provided, it can be spawned on any terrain
        SpawnableTerrain = spawnableTerrain;
    }
}

