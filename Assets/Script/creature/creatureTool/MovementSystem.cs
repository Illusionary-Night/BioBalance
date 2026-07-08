using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;


public static class MovementSystem
{
    public static void Initialize(CreatureData data)
    {

        data.movementState = CreatureMovementState.Idle;
        data.destination = Vector2Int.zero;
        data.path.Clear();
        data.currentPathIndex = 0;
        data.ClearMovementEvents();
    }
    public static void MoveTo(CreatureData data, Vector2Int dest, bool isRunning)
    {
        if (data.isDead) return;

        data.destination = dest;
        data.movementState = isRunning ? CreatureMovementState.Run : CreatureMovementState.Walk;
        Navigate(data);
    }

    private static void Navigate(CreatureData data)
    {
        Vector2Int start = Vector2Int.RoundToInt(data.position);
        List<Vector2Int> rawPath = AStar.FindPath(start, data.destination, TerrainGenerator.Instance.GetDefinitionMap().GetTerrainWeight);
        data.path.Clear();
        data.currentPathIndex = 0;
        if (rawPath == null || rawPath.Count == 0)
        {
            return;
        }
        foreach (var p in rawPath)
        {
            data.path.Add(new Vector2(p.x, p.y));
        }
    }

    public static void OnTick(CreatureData data, Rigidbody2D rb)
    {
        if (data.isDead || !data.isMoving || data.isStunned || data.path.Count == 0)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 currentActualPos = rb.position;

        if (data.currentPathIndex < data.path.Count)
        {
            Vector2 target = data.path[data.currentPathIndex];
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
                data.currentPathIndex++;
            }
        }
        else
        {
            // 已經走完 Path 了
            rb.linearVelocity = Vector2.zero;
        }

        // 抵達最終目的地判定
        if (Vector2.Distance(currentActualPos, data.destination) < 0.5f)
        {
            data.movementState = CreatureMovementState.Idle;

            //TODO: 事件呼叫完成，但我覺得事件寫法好像怪怪的
        }
    }
    //TODO: 之後由data自己管理
    private static float GetCurrentSpeed(CreatureData data)
    {
        return data.movementState switch
        {
            CreatureMovementState.Run => data.speed * 2f,
            CreatureMovementState.Walk => data.speed * 1f,
            _ => 0f
        };
    }
    public static Vector2Int GetRoundedPosition(CreatureData data)
    {
        return Vector2Int.RoundToInt(data.position);
    }

    public static Vector2Int GetMovementDestination(CreatureData data)
    {
        if (data.destination != null)
        {
            return data.destination;
        }
        else
        {
            return Vector2Int.zero;
        }

    }
    // TODO: 怪怪的，理論上我這邊不能直接對RB操作？
    public static void Pushed(CreatureData data, Vector2 direction, float strength)
    {
        rb.AddForce(direction.normalized * strength, ForceMode2D.Impulse);
    }
}