using UnityEngine;

public class Carrion : Edible
{
    public override int LifeTime { get; protected set; } = 500;
    public override float NutritionalValue => 20f;
	public override FoodType Type => FoodType.Carrion;

    protected override EntityData.SpawnableEntityType GetEntityType()
    {
        return EntityData.SpawnableEntityType.Carrion;
    }
}