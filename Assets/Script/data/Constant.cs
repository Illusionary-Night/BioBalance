using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using UnityEngine;


public class constantData : MonoBehaviour
{
    public const int NORMAL_SIZE = 1;
    public const int STOCK_LIMIT = 5;

    public const int TICKS_PER_HOUR = 60;   // 每小時 60 Ticks
    public const int HOURS_PER_DAY = 30;

    public const int UNIVERSAL_ACTION_COOLDOWN = 20;

}


public enum LifeState { Infant, Juvenile, Adult, Elder }
public enum BodyType
{
    Small,
    Medium,
    Large
}

public enum ActionType
{
    Daze,
    Wander,
    Eat,
    Sleep,
    Reproduce,
    Attack,
    Flee,
    Retaliate
}

public enum FoodType
{
    Grass,
    Meat,
    Carrion
}
[System.Serializable]
public struct CreatureAttributes
{
    public float size;
    public float speed;
    public float max_health;
    public float reproduction_rate;
    public float attack_power;
    public float lifespan;
    public float perception_range;
    public int sleeping_head;
    public int sleeping_tail;
}
public static class AttributesCalculator{
    public static float CalculateHungerRate(float size, float speed, float attack_power)
    {
        return (size * speed + attack_power/20)/100;
    }
    public static float CalculateMaxHunger(float size, float base_health, List<FoodType> foods)
    {
        float dietFactor = 1.0f;
        if (foods.Contains(FoodType.Grass) && (foods.Contains(FoodType.Meat) || foods.Contains(FoodType.Carrion))) dietFactor = 1.0f;
        else if (foods.Contains(FoodType.Meat) || foods.Contains(FoodType.Carrion)) dietFactor = 1.2f;
        else if (foods.Contains(FoodType.Grass)) dietFactor = 0.8f;
        return size * base_health * dietFactor;
    }
    public static float CalculateReproductionInterval(float size, float base_health)
    {
        return size * base_health;
    }
    public static float CalculateHealthRegeneration(float base_health, float size, float sleeping_time)
    {
        return base_health * sleeping_time / size /100000;
    }
}

// 地形類型定義
public enum TerrainType
{
    Grass,      // 草地
    Sand,       // 沙地
    Rock,       // 岩石
    Swamp,      // 沼澤
    Barrier,    // 障礙物，不可通行
    Water,      // 水域
    Lava,       // 熔岩
    Ice         // 冰面
}

public static class DefaultTerrainCosts
{
    public static readonly Dictionary<TerrainType, float> TerrainCosts = new Dictionary<TerrainType, float>
    {
        { TerrainType.Grass, 1.0f },
        { TerrainType.Sand, 1.5f },
        { TerrainType.Rock, 10f },
        { TerrainType.Swamp, 2.5f },
        { TerrainType.Barrier, float.MaxValue }, // 不可通行
        { TerrainType.Water, 2.5f }, // 不可通行
        { TerrainType.Lava, 10.0f },
        { TerrainType.Ice, 1.2f }
    };

}

public enum Direction
{
    None, North, South, East, West, Northwest, Southwest, Northeast, Southeast
}

public enum CreatureBase
{
    Slime,
    Goat, 
    IceDragon,
    Tiger
}