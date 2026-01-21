using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class EnvEntityManager : ITickable
{
    // ========== ONLY FOR TEMPORARY TESTING ==========
    // Define spawn range constants (!!BOTH INCLUSIVE!!)
    private const int MINSPAWNRANGE = 0;
    private const int MAXSPAWNRANGE = 500;
    // ========== ONLY FOR TEMPORARY TESTING ==========

    public EnvEntityManager()
    {
        Initialize();
    }

    public Transform EnvironmentEntities { get; private set; }

    // Dictionary to hold entities with their positions (by type)
    private readonly Dictionary<EntityData.SpawnableEntityType, Dictionary<Vector2Int, GameObject>> _entityDict = new();
    // Dictionary to hold entity pools (by type)
    private readonly Dictionary<EntityData.SpawnableEntityType, IEntityPool> _entityPool = new();

    private void Initialize()
    {
        GameObject env_entites_prefab = Resources.Load<GameObject>("Prefabs/Parents/EnvironmentEntities");
        EnvironmentEntities = UnityEngine.Object.Instantiate(env_entites_prefab).transform;

        // Initialize entity dictionaries and pools
        foreach (EntityData.SpawnableEntityType type in Enum.GetValues(typeof(EntityData.SpawnableEntityType)))
        {
            SpawnableEntity entityData = EntityData.GetSpawnableEntity(type);
            _entityDict.Add(type, new Dictionary<Vector2Int, GameObject>());

            // 使用反射創建正確的泛型池
            Type poolType = typeof(EntityPool<>).MakeGenericType(entityData.ClassType);
            IEntityPool poolInstance = (IEntityPool)Activator.CreateInstance(poolType, new object[] { type, 1000});
            _entityPool.Add(type, poolInstance);
        }
    }

    public void OnEnable()
    {
        TickManager.Instance.RegisterTickable(OnTick);
    }

    public void OnDisable()
    {
        TickManager.Instance.UnregisterTickable(OnTick);
    }

    public void OnTick()
    {
        RandomlySpawnEntity(EntityData.SpawnableEntityType.Grass);
    }

    /// <summary>
    /// 取得指定類型的實體數量
    /// </summary>
    public int GetEntityCount(EntityData.SpawnableEntityType type)
    {
        if (_entityDict.TryGetValue(type, out var dict))
        {
            return dict.Count;
        }
        return 0;
    }

    /// <summary>
    /// 取得指定類型的實體池
    /// </summary>
    public IEntityPool GetPool(EntityData.SpawnableEntityType type)
    {
        if (_entityPool.TryGetValue(type, out var pool))
        {
            return pool;
        }
        return null;
    }

    private void RandomlySpawnEntity(EntityData.SpawnableEntityType spawnableType)
    {
        SpawnableEntity entityData = EntityData.GetSpawnableEntity(spawnableType);
        
        // 檢查是否達到最大數量限制
        if (GetEntityCount(spawnableType) >= entityData.MaxSpawnableValue) return;

        Vector2Int position = new Vector2Int(
            Random.Range(MINSPAWNRANGE, MAXSPAWNRANGE + 1),
            Random.Range(MINSPAWNRANGE, MAXSPAWNRANGE + 1)
        );

        // Check if position is occupied
        if (_entityDict[spawnableType].ContainsKey(position)) return;

        // Check terrain type
        var random_positions = GetRandomPosition(position, 3, Random.Range(1, 5));
        foreach (var pos in random_positions)
        {
            SpawnEntity(spawnableType, pos);
        }
    }

    private List<Vector2Int> GetRandomPosition(Vector2Int position, int r, int n)
    {
        List<Vector2Int> positions = new();
        HashSet<Vector2Int> used_position = new();
        uint tries = 0;
        while (positions.Count < n && tries < n * 10)
        {
            float angle = Random.Range(0f, 360f);
            float radius = Random.Range(0f, r);
            Vector2Int new_position = position + new Vector2Int(
                Mathf.RoundToInt(radius * Mathf.Cos(angle * Mathf.Deg2Rad)),
                Mathf.RoundToInt(radius * Mathf.Sin(angle * Mathf.Deg2Rad))
            );
            if (!used_position.Contains(new_position))
            {
                used_position.Add(new_position);
                positions.Add(new_position);
            }
            tries++;
        }
        return positions;
    }

    /// <summary>
    /// 在指定位置生成實體
    /// </summary>
    /// <returns>是否成功生成</returns>
    public bool SpawnEntity(EntityData.SpawnableEntityType spawnableType, Vector2Int pos)
    {
        // 檢查位置是否已被佔用
        if (_entityDict[spawnableType].ContainsKey(pos)) return false;

        SpawnableEntity entityData = EntityData.GetSpawnableEntity(spawnableType);

        // 檢查地形是否允許生成
        if (entityData.SpawnableTerrain != null && entityData.SpawnableTerrain.Count > 0)
        {
            TerrainType currentTerrain = TerrainGenerator.Instance.GetDefinitionMap().GetTerrain(pos);
            bool canSpawn = false;
            foreach (var terrainType in entityData.SpawnableTerrain)
            {
                if (currentTerrain == terrainType)
                {
                    canSpawn = true;
                    break;
                }
            }
            if (!canSpawn) return false;
        }

        // 從物件池取得實體
        MonoBehaviour entity = _entityPool[spawnableType].GetEntity(pos, EnvironmentEntities);
        if (entity == null) return false;

        // 記錄到字典中
        _entityDict[spawnableType].Add(pos, entity.gameObject);

        // 如果實體有 Initialize 方法，呼叫它
        if (entity is Edible edible)
        {
            edible.Initialize();
        }

        return true;
    }

    /// <summary>
    /// 移除指定位置的實體並回收到物件池
    /// </summary>
    public bool RemoveEntity(EntityData.SpawnableEntityType spawnableType, Vector2Int pos)
    {
        if (!_entityDict[spawnableType].TryGetValue(pos, out GameObject entityObj))
        {
            return false;
        }

        // 從字典中移除
        _entityDict[spawnableType].Remove(pos);

        // 回收到物件池
        MonoBehaviour entity = entityObj.GetComponent<MonoBehaviour>();
        if (entity != null)
        {
            _entityPool[spawnableType].ReleaseEntity(entity);
        }

        return true;
    }

    /// <summary>
    /// 取得指定位置的實體
    /// </summary>
    public T GetEntity<T>(EntityData.SpawnableEntityType spawnableType, Vector2Int position) where T : MonoBehaviour
    {
        if (_entityDict.TryGetValue(spawnableType, out var entityPositions))
        {
            if (entityPositions.TryGetValue(position, out var entityObj))
            {
                return entityObj.GetComponent<T>();
            }
        }
        return null;
    }

    /// <summary>
    /// 清空所有實體並清理物件池
    /// </summary>
    public void ClearAll()
    {
        foreach (var type in _entityDict.Keys)
        {
            _entityDict[type].Clear();
            _entityPool[type].ClearPool();
        }
    }
}
