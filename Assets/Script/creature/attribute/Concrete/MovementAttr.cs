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
}
