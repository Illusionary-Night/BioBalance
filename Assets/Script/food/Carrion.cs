using UnityEngine;

public class Grass : Edible
{
	public override void Initialize(Vector2 position)
	{
		this.lifeSpan = 300;
		this.nutritionalValue = 5f;
		this.transform.position = position;
		this.Type = FoodType.Meat;
	}
}