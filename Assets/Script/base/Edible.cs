using NUnit.Framework;
using System.Xml.Serialization;
using UnityEngine;

// Abstract base class for all edible objects
public abstract class Edible : MonoBehaviour, ITickable
{
    [SerializeField]
    // Unique identifier for the edible object
    public string UUID { get; protected set; }
    [SerializeField]
    public abstract int LifeTime { get; protected set; }
    [SerializeField]
    // The remaining lifespan of the object (tick)
    protected int LifeSpan { get; private set; }

    // The amount of hunger this object restores when eaten.
    public abstract float NutritionalValue { get; }

    // The category of this food (e.g., Plant or Meat).
    public abstract FoodType Type { get; }

    // 記錄生成時的位置，用於移除時使用
    protected Vector2Int spawnPosition;

    public virtual void Initialize()
    {
        UUID = System.Guid.NewGuid().ToString();
        spawnPosition = Vector2Int.RoundToInt(transform.position);
        LifeSpan = LifeTime;
    }

    public void OnEnable()
    {
        Manager.Instance.TickManager?.RegisterTickable(OnTick);
    }

    // This method is called once per tick by the Manager.
    public virtual void OnTick()
    {
        LifeSpan--;
        if (LifeSpan <= 0)
        {
            NaturalDespawn();
        }
    }

    public void OnDisable()
    {
        Manager.Instance.TickManager?.UnregisterTickable(OnTick);
    }

    protected virtual void NaturalDespawn ()
    {
        RemoveFromManager();
    }

    public virtual void Eaten()
    {
        RemoveFromManager();
    }

    /// <summary>
    /// 從 EnvEntityManager 中移除自己
    /// </summary>
    protected virtual void RemoveFromManager()
    {
        // 取得對應的 SpawnableEntityType
        EntityData.SpawnableEntityType entityType = GetEntityType();
        
        // 通知 EnvEntityManager 移除
        Manager.Instance?.EnvEntityManager?.RemoveEntity(entityType, spawnPosition);
    }

    /// <summary>
    /// 子類需要實作此方法以返回對應的 SpawnableEntityType
    /// </summary>
    protected abstract EntityData.SpawnableEntityType GetEntityType();
}