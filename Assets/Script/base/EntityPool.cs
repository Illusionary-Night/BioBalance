using UnityEngine;
using UnityEngine.Pool;
using System;

public class EntityPool<T> : IEntityPool where T : MonoBehaviour
{
    private readonly SpawnableEntity _entityData;
    private GameObject _prefab;
    private Transform _poolParent;

    private ObjectPool<T> pool;

    public int CountActive => pool.CountActive;
    public int CountInactive => pool.CountInactive;
    public int CountAll => pool.CountAll;

    public EntityPool(EntityData.SpawnableEntityType spawnableType, int defaultCapacity = 1000)
    {
        _entityData = EntityData.GetSpawnableEntity(spawnableType);

        EnsureInitialized();

        pool = new ObjectPool<T>(
            createFunc: CreateEntity,
            actionOnGet: ActionOnGet,
            actionOnRelease: ActionOnRelease,
            actionOnDestroy: ActionOnDestroy,
            collectionCheck: false,
            defaultCapacity: defaultCapacity,
            maxSize: Math.Min(_entityData.MaxSpawnableValue, 100_000)
        );
    }

    private void EnsureInitialized()
    {
        if (_prefab == null)
        {
            _prefab = Resources.Load<GameObject>(_entityData.PrefabPath);
            if (_prefab == null)
            {
                Debug.LogError($"EntityPool<{typeof(T).Name}>: Failed to load prefab from {_entityData.PrefabPath}");
            }
        }

        if (_poolParent == null)
        {
            GameObject parentObj = new("[" + typeof(T).Name + "Pool]");
            _poolParent = parentObj.transform;
            UnityEngine.Object.DontDestroyOnLoad(parentObj);
        }
    }

    // 泛型版本（供知道具體類型時使用）
    public T GetEntityTyped()
    {
        EnsureInitialized();
        return pool.Get();
    }

    public T GetEntityTyped(Vector2Int position, Transform parent = null)
    {
        EnsureInitialized();

        T entity = GetEntityTyped();
        if (parent != null) entity.transform.SetParent(parent);
        entity.transform.position = new Vector3(position.x, position.y, 0);
        return entity;
    }

    public void ReleaseEntityTyped(T entity)
    {
        if (entity != null) pool.Release(entity);
    }

    // IEntityPool 介面實作（供動態調用時使用）
    MonoBehaviour IEntityPool.GetEntity()
    {
        return GetEntityTyped();
    }

    MonoBehaviour IEntityPool.GetEntity(Vector2Int position, Transform parent)
    {
        return GetEntityTyped(position, parent);
    }

    void IEntityPool.ReleaseEntity(MonoBehaviour entity)
    {
        if (entity is T typedEntity)
        {
            ReleaseEntityTyped(typedEntity);
        }
        else if (entity != null)
        {
            Debug.LogWarning($"EntityPool<{typeof(T).Name}>: Cannot release entity of type {entity.GetType().Name}");
        }
    }

    public void ClearPool()
    {
        pool.Clear();
        if (_poolParent != null)
        {
            UnityEngine.Object.Destroy(_poolParent.gameObject);
            _poolParent = null;
        }

        _prefab = null;
    }

    private T CreateEntity()
    {
        EnsureInitialized();

        if (_prefab == null)
        {
            Debug.LogError($"EntityPool<{typeof(T).Name}>: Prefab is null, cannot create entity.");
            return null;
        }

        GameObject obj = UnityEngine.Object.Instantiate(_prefab, _poolParent);
        obj.name = "Pooled" + _prefab.name;
        obj.SetActive(false);

        T entity = obj.GetComponent<T>();
        if (entity == null)
        {
            Debug.LogError($"EntityPool<{typeof(T).Name}>: Prefab does not contain component of type {typeof(T).Name}");
            UnityEngine.Object.Destroy(obj);
            return null;
        }
        return entity;
    }

    private void ActionOnGet(T entity)
    {
        if (entity == null) return;

        entity.gameObject.SetActive(true);
        entity.transform.SetParent(null);
        entity.transform.localScale = Vector3.one;
    }

    private void ActionOnRelease(T entity)
    {
        if (entity == null) return;

        entity.StopAllCoroutines();

        entity.gameObject.SetActive(false);
        entity.transform.SetParent(_poolParent);
        entity.transform.position = Vector3.zero;
        entity.transform.rotation = Quaternion.identity;
    }

    private void ActionOnDestroy(T entity)
    {
        if (entity != null && entity.gameObject != null)
        {
            UnityEngine.Object.Destroy(entity.gameObject);
        }
    }
}
