using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

//TODO: 事件的部分有漏 OnMovementComplete invoke
public static class MovementSystem
{
    public static void Initialize(CreatureData data)
    {

        data.movement.movementState = CreatureMovementState.Idle;
        data.movement.destination = Vector2Int.zero;
        data.movement.path.Clear();
        data.movement.currentPathIndex = 0;
        data.movement.ClearMovementEvents();
    }
    public static void MoveTo(Creature creature, Vector2Int dest, bool isRunning)
    {
        if (creature.data.isDead) return;
        creature.data.movement.destination = dest;
        creature.data.movement.movementState = isRunning ? CreatureMovementState.Run : CreatureMovementState.Walk;
        Navigate(creature.data);
    }

    private static void Navigate(CreatureData data)
    {
        Vector2Int start = Vector2Int.RoundToInt(data.movement.position);
        List<Vector2Int> rawPath = AStar.FindPath(start, data.movement.GridDestination, TerrainGenerator.Instance.GetDefinitionMap().GetTerrainWeight);
        data.movement.path.Clear();
        data.movement.currentPathIndex = 0;
        if (rawPath == null || rawPath.Count == 0)
        {
            return;
        }
        foreach (var p in rawPath)
        {
            data.movement.path.Add(new Vector2(p.x, p.y));
        }
    }

    public static void OnTick(Creature creature, Rigidbody2D rb)
    {
        CreatureData data = creature.data;
        if (data.isDead || !data.isMoving || data.isStunned || data.movement.path.Count == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 currentActualPos = rb.position;

        if (data.movement.currentPathIndex < data.movement.path.Count)
        {
            Vector2 target = data.movement.path[data.movement.currentPathIndex];
            Vector2 direction = (target - currentActualPos).normalized;

            float currentSpeed = GetCurrentSpeed(data);
            rb.linearVelocity = direction * currentSpeed;

            // 轉向處理
            if (direction.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rb.MoveRotation(angle);
            }

            float dynamicRadius = Mathf.Max(0.2f, currentSpeed * Time.fixedDeltaTime * 1.5f);
            if (Vector2.Distance(currentActualPos, target) < dynamicRadius)
            {
                data.movement.currentPathIndex++;
            }
        }
        else
        {
            // 已經走完 Path 了
            rb.linearVelocity = Vector2.zero;
        }

        // 抵達最終目的地判定
        if (Vector2.Distance(currentActualPos, data.movement.destination) < 0.5f)
        {
            data.movement.movementState = CreatureMovementState.Idle;

            //TODO: 事件呼叫完成，但我覺得事件寫法好像怪怪的
            data.movement.TriggerMovementComplete(data.movement.GridDestination);
        }
    }
    //TODO: 之後由data自己管理
    private static float GetCurrentSpeed(CreatureData data)
    {
        return data.movement.movementState switch
        {
            CreatureMovementState.Run => data.speed * 2f,
            CreatureMovementState.Walk => data.speed * 1f,
            _ => 0f
        };
    }
    public static void Pushed(Creature creature, Vector2 direction, float strength)
    {
        creature.rb.AddForce(direction.normalized * strength, ForceMode2D.Impulse);
    }
}