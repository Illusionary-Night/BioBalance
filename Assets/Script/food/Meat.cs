using UnityEngine;

public class Meat : Edible
{
    public override int LifeTime { get; protected set; } = 300;
    public override float NutritionalValue => 30f;
    public override FoodType Type => FoodType.Meat;

    protected override EntityData.SpawnableEntityType GetEntityType()
    {
        return EntityData.SpawnableEntityType.Meat;
    }

    protected override void NaturalDespawn()
    {
        Manager.Instance?.EnvEntityManager.SpawnEntity(EntityData.SpawnableEntityType.Carrion, transform.position);
        base.NaturalDespawn();
    }
}