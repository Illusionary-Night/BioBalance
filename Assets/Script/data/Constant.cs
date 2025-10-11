using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public enum BodyType
{
    Small,
    Medium,
    Large
}

public enum DietType
{
    Herbivore,  //草食
    Carnivore,  //肉食
    Omnivore    //雜食
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
}
public struct CreatureAttributes
{
    public float size;
    public float speed;
    public float base_health;
    public float reproduction_rate;
    public float attack_power;
    public float lifespan;
    public float variation;
    public int[] sleeping_cycle;
    public DietType Diet { get; set; }
    public BodyType Body { get; set; }     //最終體型
    public List<Creature> food_list;       //新增食物列表
    public List<Creature> predator_list;   //新增天敵列表
    public List<ActionType> action_list;
    
}

public static class AttributesCalculator{
    public static float CalculateHungerRate(float size, float speed, float attack_power)
    {
        return size * speed + attack_power;
    }
    public static float CalculateMaxHunger(float size, float base_health, DietType diet)
    {
        float dietFactor = new float[] { 0.8f, 1.2f, 1.0f }[(int)diet];
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