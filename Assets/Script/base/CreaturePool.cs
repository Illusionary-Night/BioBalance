using UnityEngine;
using UnityEngine.Pool;

public static class CreaturePool
{
    // Object pool for Creature instances
    private static ObjectPool<Creature> pool = new(
        createFunc: CreateCreature,
        actionOnGet: ActionOnGet,
        actionOnRelease: ActionOnRelease,
        actionOnDestroy: ActionOnDestroy,
        collectionCheck: false,
        defaultCapacity: 200,
        maxSize: 10000
    );

    public static Creature GetCreature()
    {
        return pool.Get();
    }

    public static void ReleaseCreature(Creature creature)
    {
        pool.Release(creature);
    }

    // Factory method to create a new Creature instance
    private static Creature CreateCreature()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Creature");
        return GameObject.Instantiate(prefab).GetComponent<Creature>();
    }

    private static void ActionOnGet(Creature creature)
    {         
        // 初始化或重置 Creature 狀態
    }

    private static void ActionOnRelease(Creature creature)
    {
        // 清理 Creature 狀態
    }

    private static void ActionOnDestroy(Creature creature)
    {
        // 銷毀 Creature 資源
        Object.Destroy(creature.gameObject);
    }
}
