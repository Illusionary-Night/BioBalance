using UnityEngine;

public class Meat : Edible
{
    public override void Initialize(Vector2 position)
    {
        this.lifeSpan = 150;
        this.nutritionalValue = 30f;
        this.transform.position = position;
        this.Type = FoodType.Meat;
    }

    // Future enhancement: Could add logic here to turn into Carrion when lifespan is empty or low.
}