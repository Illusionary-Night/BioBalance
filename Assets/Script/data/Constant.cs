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
    Retaliate,
    Flock,
    Mating
}

public enum FoodType
{
    Grass,
    Meat,
    Carrion
}
//TODO: 獨立出來
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
    public Gender gender;
    public String UUID;
    public float[] colorGenes;
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

// TODO: 幾個不會動的state要不要整合在一起
public enum CreatureMovementState
{
    None = 0,
    Sleep = 1,
    Idle = 2,          // 待機
    Walk = 3,        // 散步
    Run = 4,       // 跑步
    Stunned = 5,      // 暈眩 (AI 停用，物理接管)
}

public enum Gender
{
    None,
    Male,
    Female,
}
public enum ReproductionType
{
    Asexual, // 無性：初代給固定性別（例如 Gender.None 或統一是 Female）
    Sexual   // 有性：初代直接 50/50 盲抽
}