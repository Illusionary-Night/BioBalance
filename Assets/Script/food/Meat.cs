using UnityEngine;

public class Meat : Edible
{
    public override int LifeSpan { get; protected set; } = 300;
    public override float NutritionalValue => 30f;
    public override FoodType Type => FoodType.Meat;

}