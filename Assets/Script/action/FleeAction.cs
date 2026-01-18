using System.Collections.Generic;
using UnityEngine;

public class FleeAction : ActionBase
{
    // 逃跑觸發的威脅距離閾值
    private const float THREAT_DISTANCE_THRESHOLD = 5f;
    // 逃跑時的隨機偏移角度（用於避障）
    private const float OBSTACLE_AVOIDANCE_ANGLE = 30f;
    // 逃跑距離（感應範圍內的隨機距離）
    private const float MIN_FLEE_RATE = 0.4f;
    private const float MAX_FLEE_RATE = 0.7f;

    public override ActionType Type => ActionType.Flee;

    /// <summary>
    /// 前置條件檢查：
    /// 1. 正在被攻擊（受擊方向不為 None）
    /// 2. 感應範圍內有捕食者（該捕食者的食物名單包含自己）且距離低於閾值
    /// </summary>
    public override bool IsConditionMet(Creature creature)
    {
        // 條件 1：正在被攻擊
        if (creature.UnderAttack())
        {
            return true;
        }

        // 條件 2：感應範圍內有捕食者且距離過近
        List<Creature> predators = GetNearbyPredators(creature);
        if (predators.Count > 0)
        {
            // 檢查最近的捕食者距離是否低於閾值
            Creature nearestPredator = predators[0]; // 已按距離排列
            float distance = Vector2.Distance(creature.transform.position, nearestPredator.transform.position);
            return distance < THREAT_DISTANCE_THRESHOLD;
        }

        return false;
    }

    public override float GetWeight(Creature creature)
    {
        // 逃跑優先級較高，特別是在受攻擊時
        if (creature.UnderAttack())
        {
            return 5.0f;
        }
        return 1.3f;
    }

    public override bool IsSuccess(Creature creature)
    {
        // 逃跑行為始終嘗試執行
        return true;
    }

    public override void Execute(Creature creature, ActionContext context = null)
    {
        // 先重置受擊狀態，避免條件持續滿足導致重複觸發
        creature.GetAndResetUnderAttackDirection();

        // 計算威脅參考點
        Vector2 threatPosition = GetThreatReferencePoint(creature);
        
        // 計算逃跑方向（反方向）
        Vector2 fleeDirection = CalculateFleeDirection(creature, threatPosition);
        
        // 計算逃跑目標位置
        Vector2Int fleeTarget = CalculateFleeTarget(creature, fleeDirection);

        // 檢查目標是否有效
        Vector2Int currentPos = creature.GetRoundedPosition();
        if (fleeTarget == currentPos)
        {
            // 無法逃跑，直接完成
            context?.Complete();
            return;
        }

        // 使用狀態機註冊移動回調
        var stateMachine = creature.GetStateMachine();

        // 使用 flag 防止重複執行
        bool hasCompleted = false;

        System.Action<Vector2Int> onArrived = (arrivedPosition) =>
        {
            // 防止重複執行
            if (hasCompleted)
            {
                return;
            }

            // 檢查 Context 是否仍然有效
            if (context != null && !context.IsValid)
            {
                return;
            }

            hasCompleted = true;

            // 標記 Action 完成
            context?.Complete();
        };

        // 透過狀態機註冊回調（自動管理清理）
        stateMachine.RegisterMovementCallback(onArrived);
        creature.MoveTo(fleeTarget);
    }

    /// <summary>
    /// 取得附近的捕食者清單（按距離排序）
    /// </summary>
    private List<Creature> GetNearbyPredators(Creature creature)
    {
        return Perception.Creatures.GetAllTargets(creature, creature.PredatorIDList);
    }

    /// <summary>
    /// 取得威脅參考點：
    /// - 被多個捕食者追逐時，選擇最近的作為參考
    /// - 正在被攻擊時，使用受擊方向
    /// </summary>
    private Vector2 GetThreatReferencePoint(Creature creature)
    {
        Vector2 creaturePos = creature.transform.position;

        // 處理捕食者威脅（優先，因為受擊方向已被重置）
        List<Creature> predators = GetNearbyPredators(creature);
        if (predators.Count > 0)
        {
            // 選擇最近的捕食者作為威脅參考點
            return predators[0].transform.position;
        }

        // 無明確威脅時，隨機選擇一個方向逃跑
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return creaturePos + new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
    }

    /// <summary>
    /// 計算逃跑方向（威脅的反方向）
    /// </summary>
    private Vector2 CalculateFleeDirection(Creature creature, Vector2 threatPosition)
    {
        Vector2 creaturePos = creature.transform.position;
        Vector2 fleeDirection = (creaturePos - threatPosition).normalized;

        // 如果方向為零向量，隨機選擇一個方向
        if (fleeDirection.sqrMagnitude < 0.001f)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            fleeDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
        }

        return fleeDirection;
    }

    /// <summary>
    /// 計算逃跑目標位置，包含障礙物偏移嘗試
    /// </summary>
    private Vector2Int CalculateFleeTarget(Creature creature, Vector2 fleeDirection)
    {
        Vector2 creaturePos = creature.transform.position;
        float fleeDistance = Random.Range(creature.PerceptionRange * MIN_FLEE_RATE, creature.PerceptionRange * MAX_FLEE_RATE);

        // 嘗試直接逃跑方向
        Vector2Int targetPos = Vector2Int.RoundToInt(creaturePos + fleeDirection * fleeDistance);

        // 若前方有障礙，嘗試偏移角度
        if (!IsPositionWalkable(targetPos))
        {
            // 嘗試左偏
            Vector2 leftDirection = RotateVector(fleeDirection, OBSTACLE_AVOIDANCE_ANGLE);
            Vector2Int leftTarget = Vector2Int.RoundToInt(creaturePos + leftDirection * fleeDistance);
            if (IsPositionWalkable(leftTarget))
            {
                return leftTarget;
            }

            // 嘗試右偏
            Vector2 rightDirection = RotateVector(fleeDirection, -OBSTACLE_AVOIDANCE_ANGLE);
            Vector2Int rightTarget = Vector2Int.RoundToInt(creaturePos + rightDirection * fleeDistance);
            if (IsPositionWalkable(rightTarget))
            {
                return rightTarget;
            }

            // 若兩側都不可行，縮短逃跑距離
            fleeDistance *= 0.5f;
            targetPos = Vector2Int.RoundToInt(creaturePos + fleeDirection * fleeDistance);
        }

        return targetPos;
    }

    /// <summary>
    /// 旋轉向量指定角度
    /// </summary>
    private Vector2 RotateVector(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }

    /// <summary>
    /// 檢查位置是否可通行
    /// </summary>
    private bool IsPositionWalkable(Vector2Int position)
    {
        // 整合實際的地形檢查系統
        if (TerrainGenerator.Instance == null) return true;
        
        return TerrainGenerator.Instance.GetDefinitionMap().IsWalkable(position);
    }
}