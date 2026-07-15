using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveAction : ActionBase
{
    public override ActionType Type => ActionType.Wander;
    //public override int Cooldown => 10;
    //[SerializeField] private static readonly int MoveDistance = 100;

    public override bool IsConditionMet(Creature creature)
    {
        // 永遠可以移動
        return true;
    }

    public override float GetWeight(Creature creature)
    {
        return 0.3f;
        // 根據飢餓值決定移動權重，飢餓值越高，移動權重越高
        //return (creature.Hunger / creature.MaxHunger) / Perception.Creatures.CountTargetNumber(creature, creature.SpeciesID);
    }

    public override bool IsSuccess(Creature creature)
    {
        return Random.Range(0, 9) < 9;
    }

    public override void Execute(Creature creature, ActionContext context = null)
    {
        //Debug.Log("move");
        // 隨機移動到鄰近位置
        Vector2Int currentPosition = creature.GetRoundedPosition();
        int rangeInt = Mathf.FloorToInt(creature.perceptionRange);
        Vector2Int randomDisplacement = new(Random.Range(-rangeInt, rangeInt + 1), Random.Range(-rangeInt, rangeInt + 1));
        //Vector2Int randomDisplacement = new(Random.Range(-MoveDistance, MoveDistance + 1), Random.Range(-MoveDistance, MoveDistance + 1));
        Vector2Int newPosition = currentPosition + randomDisplacement;

        // 呼叫Creature自行導航地點
        creature.MoveTo(newPosition);
    }
}