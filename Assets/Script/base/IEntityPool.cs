using UnityEngine;

public interface IEntityPool
{
    int CountActive { get; }
    int CountInactive { get; }
    int CountAll { get; }

    MonoBehaviour GetEntity();
    MonoBehaviour GetEntity(Vector2Int position, Transform parent = null);
    void ReleaseEntity(MonoBehaviour entity);
    void ClearPool();
}