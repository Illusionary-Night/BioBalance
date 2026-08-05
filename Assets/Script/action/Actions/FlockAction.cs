using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

class FlockAction : ActionBase
{
    public override ActionType Type => ActionType.Flock;

    public override bool IsConditionMet(Creature creature)
    {
        // 1. 基本安全檢查：沒暈眩、沒死、沒被鎖定
        if (creature.isStunned || creature.isDead) return false;

        // 2. 社交檢查：找尋感知範圍內的同類
        var neighbors = Perception.Creatures.GetAllTargets(creature, creature.speciesID);
        if (neighbors.Count < 3) return false;

        // 3. 生理檢查：肚子不能太餓
        if (creature.data.hunger.Percentage < 0.3f) return false;

        return true;
    }
    public override float GetWeight(Creature creature)
    {
        float baseWeight = 0.5f; // 基礎社交傾向

        // 1. 同類數量影響 (假設 neighbors 是感測到的同類清單)
        int neighborCount = Perception.Creatures.GetAllTargets(creature, creature.speciesID).Count;
        float densityBonus = Mathf.Clamp(neighborCount * 0.15f, 0, 0.6f);

        // 2. 飢餓度懲罰 (肚子越餓越不想社交)
        float hungerPenalty = Mathf.InverseLerp(0.2f, 0.6f, creature.data.hunger.Percentage);

        // 3. 行為慣性 (延續感)
        float inertia = (creature.currentAction == ActionType.Flock) ? 0.4f : 0f;

        // 最終權重
        return (baseWeight + densityBonus + inertia) * hungerPenalty;
    }

    public override bool IsSuccess(Creature creature)
    {
        return Random.Range(0, 9) < 7;
    }

    public override void Execute(Creature creature, ActionContext context)
    {
        // 1. 計算群聚方向 (Boids 向量)
        Vector2 flockDir = GetFlockingDirection(creature, Perception.Creatures.GetAllTargets(creature, creature.speciesID));

        // 2. 加上避障與隨機擾動，選定一個 5 格外的點
        Vector2Int targetPos = creature.GetRoundedPosition() + Vector2Int.RoundToInt(flockDir * 5f);

        // 3. 呼叫你的 Movement 系統，設定為 Walk
        creature.MoveTo(targetPos, isRunning: false);

        // 4. 監聽抵達事件來叫停
        creature.OnMovementComplete += OnArrived;

        void OnArrived(Vector2Int pos)
        {
            creature.OnMovementComplete -= OnArrived;
            context.Complete();
        }
    }
    Vector2 GetFlockingDirection(Creature creature, List<Creature> neighbors)
    {
        Vector2 separation = Vector2.zero;
        Vector2 alignment = Vector2.zero;
        Vector2 cohesion = Vector2.zero;
        Vector2 centerOfMass = Vector2.zero;

        foreach (var n in neighbors)
        {
            // 1. 分離：遠離太近的同類
            Vector2 diff = (Vector2)creature.transform.position - (Vector2)n.transform.position;
            separation += diff.normalized / diff.magnitude;

            // 2. 對齊：加總鄰居的速度向量
            alignment += n.GetComponent<Rigidbody2D>().linearVelocity;

            // 3. 凝聚：累加位置以計算中心點
            centerOfMass += (Vector2)n.transform.position;
        }

        if (neighbors.Count > 0)
        {
            centerOfMass /= neighbors.Count;
            cohesion = (centerOfMass - (Vector2)creature.transform.position).normalized;
            alignment /= neighbors.Count;
        }

        // 將三者加權結合 (權重可依物種特性調整)
        return (separation * 1.5f + alignment * 1.0f + cohesion * 1.0f).normalized;
    }
}