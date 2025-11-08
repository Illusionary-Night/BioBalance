using UnityEngine;

public class FleeAction : ActionBase
{
    public FleeAction() { }

    public override ActionType Type => ActionType.Flee;
    public override int Cooldown => 3;

    public override bool IsConditionMet(Creature creature)
    {
        return Perception.HasTarget(creature, creature.PredatorIDList);   
    }

    public override float GetWeight(Creature creature)
    {
        //(1 / 偵查範圍內同類個體數 + 1) * 0.8
        return (1f / (Perception.CountTargetNumber(creature, creature.SpeciesID) + 1)) * 0.8f;
    }

    public override bool IsSuccess(Creature creature)
    {
        return Random.value < 0.6f; // 60% 成功率
    }

    public override void Execute(Creature creature)
    {
        List<Vector2int>=AStar.FindPath();
    }
}