using System.Xml.Serialization;
using UnityEngine;

// Abstract base class for all edible objects
public abstract class Edible : MonoBehaviour, ITickable
{
    // Unique identifier for the edible object
    public string UUID { get; protected set; }
    // The remaining lifespan of the object (tick)
    public abstract int LifeSpan { get; protected set; }

    // The amount of hunger this object restores when eaten.
    public abstract float NutritionalValue { get; }

    // The category of this food (e.g., Plant or Meat).
    public abstract FoodType Type { get; }

    public void OnEnable()
    {
        Manager.OnTick += OnTick;
    }

    // This method is called once per tick by the Manager.
    public virtual void OnTick()
    {
        LifeSpan--;
        if (LifeSpan <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void OnDisable()
    {
        Manager.OnTick -= OnTick;
    }

    public virtual void Eaten()
    {
        Destroy(this.gameObject);
    }

    public void Initialize(Vector2Int position)
    {
        this.UUID = System.Guid.NewGuid().ToString();
        this.transform.position = (Vector3Int)position;
    }
}