using System.Collections.Generic;
using UnityEngine;

public class MovementAttr : IAttribute
{
    public Vector2 position;
    public Vector2 destination;
    public CreatureMovementState State { get; private set; }  //理論上這玩意除了movement system之外都不應該改他
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
    //只提供給movement state 統一修改 state，避免BUG
    public void SetStateBySystem(CreatureMovementState newState)
    {
        State = newState;
    }
}
