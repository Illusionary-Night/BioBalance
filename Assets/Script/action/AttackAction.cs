/*
 * [目前版本] 無分辨獵物種類先後順序，只以遠近定論
*/
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Perception;
using static UnityEditor.Timeline.TimelinePlaybackControls;
using static UnityEngine.GraphicsBuffer;

public class AttackAction : ActionBase
{
	public override ActionType Type => ActionType.Attack;

    public override bool IsConditionMet(Creature creature)
	{
		return Perception.Creatures.HasTarget(creature, creature.preyIDList);
	}

	public override float GetWeight(Creature creature)
	{
		return 0.8f;
	}

	public override bool IsSuccess(Creature creature)
	{
		return Random.Range(0,9)<8;
	}

	public override void Execute(Creature creature, ActionContext context = null)
	{
        Creature target = FindTarget(creature, context);
        if (target != null)
        {
            MoveAndAttack(creature, target, context);
        }
        else
        {
            // 沒有找到目標，直接標記為完成
            context?.Complete();
        }
    }
    protected virtual Creature FindTarget(Creature creature, ActionContext context)
    {
        List<Creature> optionalTargets = Perception.Creatures.GetAllTargets(creature, creature.preyIDList);
        return optionalTargets.FirstOrDefault();

    } 
    protected virtual void MoveAndAttack(Creature creature, Creature target, ActionContext context) {
        Vector2Int targetPosition = Vector2Int.RoundToInt(target.transform.position);
        Collider2D targetCollider = target.GetComponent<Collider2D>();
        if (targetCollider == null)
        {
            Debug.LogWarning("collider missing");
            return;
        }

        // 使用狀態機註冊移動回調
        var stateMachine = creature.GetStateMachine();

        System.Action<Vector2Int> onArrived = (arrivedPosition) =>
        {
            // 檢查 Context 是否仍然有效
            if (context != null && !context.IsValid)
            {
                return;
            }
            // 確認是否有目標碰撞箱
            if (IsInAttackArrange(creature, target, targetCollider))
            {
                // 檢查目標是否仍然存在
                if (target != null)
                {
                    Attack(creature, target);
                }
                else
                {
                    Debug.Log("target is null");
                }

                // 標記 Action 完成
                context?.Complete();
            }
        };

        // 透過狀態機註冊回調（自動管理清理）
        stateMachine.RegisterMovementCallback(onArrived);
        creature.MoveTo(targetPosition);
    }
    protected virtual bool IsInAttackArrange(Creature creature, Creature target, Collider2D targetCollider)
    {
        int creatureLayerMask = LayerMask.GetMask("Creature");
        Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(creature.transform.position, creature.size * 1.5f, creatureLayerMask);//會卡再改
        return potentialTargets.Any(c => c == targetCollider);
        
    }
    protected virtual void Attack(Creature creature, Creature target)
    {
        Debug.Log(creature.creatureBase + " Attack!");
        target.Hurt(creature.attackPower, creature.transform.position, creature);
    }
}