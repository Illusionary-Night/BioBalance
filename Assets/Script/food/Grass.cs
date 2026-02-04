using UnityEngine;

public class Grass : Edible
{
    [SerializeField]
    public override int LifeTime { get; protected set; } = 50000;
    [SerializeField]
    public override float NutritionalValue => 50f;
    [SerializeField]
    public override FoodType Type => FoodType.Grass;

    protected override EntityData.SpawnableEntityType GetEntityType()
    {
        return EntityData.SpawnableEntityType.Grass;
    }
}