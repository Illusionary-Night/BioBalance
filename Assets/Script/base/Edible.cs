using UnityEngine;

// Abstract base class for all edible objects
public abstract class Edible : MonoBehaviour, Tickable
{
    // The remaining lifespan of the object (tick)
    public int lifeSpan { get; protected set; }

    // The amount of hunger this object restores when eaten.
    public float nutritionalValue { get; protected set; }

    // The category of this food (e.g., Plant or Meat).
    public FoodType Type { get; protected set; }

    // This method is called once per tick by the Manager.
    public virtual void OnTick()
    {
        lifeSpan--;
        if (lifeSpan <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public virtual void Eaten()
    {
        Destroy(this.gameObject);
    }

    public abstract void Initialize(Vector2 position);
}