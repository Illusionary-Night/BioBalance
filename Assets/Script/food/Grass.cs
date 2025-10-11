using UnityEngine;

public class Grass : Edible
{
    public override void Initialize(Vector2 position)
    {
        this.lifeSpan = 500;
        this.nutritionalValue = 10f;
        this.transform.position = position;
        this.Type = FoodType.Plant;
    }
}