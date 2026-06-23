using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using UnityEngine;
public static class AttributesCalculator
{
    public static float CalculateHungerRate(float size, float speed, float attack_power)
    {
        return (size * speed + attack_power / 20) / 100;
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
    public static float CalculateHealthRegeneration(float base_health, float size)
    {
        return base_health / size / 1000;
    }
}