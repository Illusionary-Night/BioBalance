using System.Collections.Generic;
using UnityEngine;

public class DazeAction : ActionBase
{
	public override ActionType Type => ActionType.Daze;

    public override bool IsConditionMet(Creature creature)
	{
		return true;
	}

	public override float GetWeight(Creature creature)
	{
		return 0.1f;
		// 根據飢餓值決定移動權重，飢餓值越高，移動權重越高
	}

	public override bool IsSuccess(Creature creature)
	{
		return true;
	}

	public override void Execute(Creature creature, ActionContext context = null)
	{
		creature.MoveTo(creature.GetRoundedPosition());
        context?.Complete();
    }
}