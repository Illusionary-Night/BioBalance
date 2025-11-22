using System.Collections.Generic;
using UnityEngine;

public class MoveAction : ActionBase
{
	public override ActionType Type => ActionType.Move;
	public override int Cooldown => 30;
	[SerializeField] private static readonly int MoveDistance = 100;

    public override bool IsConditionMet(Creature creature)
	{
		// 永遠可以移動
		return true;
	}

	public override float GetWeight(Creature creature)
	{
		return 0.9f;
		// 根據飢餓值決定移動權重，飢餓值越高，移動權重越高
		//return (creature.Hunger / creature.MaxHunger) / Perception.Creatures.CountTargetNumber(creature, creature.SpeciesID);
	}

	public override bool IsSuccess(Creature creature)
	{
		//return true;
        // 1/2 機率成功移動
        return Random.Range(0, 2) == 0;
	}

	public override void Execute(Creature creature)
	{
        //Debug.Log("move");
        // 隨機移動到鄰近位置
        Vector2Int currentPosition = creature.GetRoundedPosition();

		Vector2Int randomDisplacement = new(Random.Range(-MoveDistance, MoveDistance + 1), Random.Range(-MoveDistance, MoveDistance + 1));
		Vector2Int newPosition = currentPosition + randomDisplacement;

        // 呼叫Creature自行導航地點
		creature.MoveTo(newPosition);
		creature.ActionCooldown = Cooldown;
    }
}