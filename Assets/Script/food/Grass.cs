using UnityEngine;

public class Grass : Edible
{
    public override int LifeSpan { get; protected set; } = 500;
    public override float NutritionalValue => 10f;
    public override FoodType Type => FoodType.Plant;
}