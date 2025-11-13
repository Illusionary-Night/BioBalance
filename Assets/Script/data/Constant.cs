using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using UnityEngine;

public enum BodyType
{
    Small,
    Medium,
    Large
}

public enum ActionType
{
    Move,
    Eat,
    Sleep,
    Reproduce,
    Attack,
    Flee
}

public enum FoodType
{
    Plant,
    Meat,
    Carrion
}
[System.Serializable]
public struct CreatureAttributes
{
    public int species_ID;
    public float size;
    public float speed;
    public float base_health;
    public float reproduction_rate;
    public float attack_power;
    public float lifespan;
    public float variation;
    public float perception_range;
    public int sleeping_head;
    public int sleeping_tail;
    public List<FoodType> FoodTypes;
    public BodyType Body { get; set; }     //最終體型
    public List<int> prey_ID_list;       //新增食物列表
    public List<int> predator_ID_list;   //新增天敵列表
    public List<ActionType> action_list;
    
}
public struct Species
{
    public CreatureAttributes attributes;
    public List<Creature> creatures;
}
public static class AttributesCalculator{
    public static float CalculateHungerRate(float size, float speed, float attack_power)
    {
        return size * speed + attack_power;
    }
    public static float CalculateMaxHunger(float size, float base_health, List<FoodType> foods)
    {
        float dietFactor = 1.0f;
        if (foods.Contains(FoodType.Plant) && (foods.Contains(FoodType.Meat) || foods.Contains(FoodType.Carrion))) dietFactor = 1.0f;
        else if (foods.Contains(FoodType.Meat) || foods.Contains(FoodType.Carrion)) dietFactor = 1.2f;
        else if (foods.Contains(FoodType.Plant)) dietFactor = 0.8f;
        return size * base_health * dietFactor;
    }
    public static float CalculateReproductionInterval(float size, float base_health)
    {
        return size * base_health;
    }
    public static float CalculateHealthRegeneration(float base_health, float size, float sleeping_time)
    {
        return base_health * sleeping_time / size;
    }
}

// 地形類型定義
public enum TerrainType
{
    Grass,      // 草地
    Sand,       // 沙地
    Rock,       // 俗頭，91度，是個斜坡
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
