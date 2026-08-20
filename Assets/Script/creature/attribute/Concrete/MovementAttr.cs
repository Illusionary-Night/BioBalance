using System.Collections.Generic;
using UnityEngine;

public class MovementAttr : IAttribute
{
    public Vector2 position;
    public Vector2 destination;
    public CreatureMovementState movementState;
    public int currentPathIndex;
    public List<Vector2> path;
    public Vector2Int GridPosition => Vector2Int.RoundToInt(position);
    public Vector2Int GridDestination => Vector2Int.RoundToInt(destination);
    public event System.Action<Vector2Int> OnMovementComplete;
    public void TriggerMovementComplete(Vector2Int destination)
    {
        // 事件接口，必須由自身自行廣播
        OnMovementComplete?.Invoke(destination);
    }
    public void ClearMovementEvents()
    {
        OnMovementComplete = null;
    }
}
