/*
 * [目前版本] 無分辨獵物種類先後順序，只以遠近定論
*/
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackAction : ActionBase
{
	public override ActionType Type => ActionType.Attack;

    public override bool IsConditionMet(Creature creature)
	{
		return Perception.Creatures.HasTarget(creature, creature.preyIDList);
	}

	public override float GetWeight(Creature creature)
	{
		return 0.5f;
	}

	public override bool IsSuccess(Creature creature)
	{
		return Random.Range(0,9)<7;
	}

	public override void Execute(Creature creature, ActionContext context = null)
	{
        List<Creature> optionalTargets = Perception.Creatures.GetAllTargets(creature, creature.preyIDList);
        if (optionalTargets.Count > 0)
        {
            Creature target = optionalTargets[0];
            Vector2Int targetPosition = Vector2Int.RoundToInt(target.transform.position);
            Collider2D targetCollider = target.GetComponent<Collider2D>();
            if (targetCollider == null) {
                Debug.LogWarning("collider missing");
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

                int creatureLayerMask = LayerMask.GetMask("Creature");
                Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(creature.transform.position, 2.0f, creatureLayerMask);//會卡再改
                bool hasTarget = potentialTargets.Any(c=>c == targetCollider);

                // 確認是否有目標碰撞箱
                if (hasTarget)
                {
                    // 檢查目標是否仍然存在
                    if (target != null)
                    {
                        Debug.Log(creature.name + " Attack!");
                        target.Hurt(creature.attackPower, creature.transform.position);
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
        else
        {
            // 沒有找到目標，直接標記為完成
            context?.Complete();
        }
    }
}