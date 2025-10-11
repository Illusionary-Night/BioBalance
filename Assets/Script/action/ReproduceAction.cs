using UnityEngine;

public class ReproduceAction : ActionBase
{
    public static readonly ReproduceAction Instance = new ReproduceAction();
    private ReproduceAction() { }

    public override ActionType Type => ActionType.Reproduce;
    public override int Cooldown => 5;

    public override bool IsConditionMet(Creature creature)
    {
        if (creature.Age < creature.Lifespan * 0.2f) return false;    // �~�֥��F20%
        if (creature.Hunger <= creature.MaxHunger * 0.5f) return false; // ���j�ȧC��50%
        if (creature.ReproductionCooldown > 0) return false; // �c�ާN�o��
        if (Perception.HasTarget(creature, creature.PredatorList)) return false; // ���񦳼ĤH
        return true;
    }

    public override float GetWeight(Creature creature)
    {
        //(1 / ���d�d�򤺦P������� + 1) * 0.8
        return (1 / (Perception.CountTargetNumber(creature,creature.ToCreatureAttribute())+1)) * 0.8f;
    }

    public override bool IsSuccess(Creature creature)
    {
        return Random.value < 0.6f; // 60% ���\�v
    }

    public override void Execute(Creature creature)
    {
        GameObject creatureObject = new GameObject(creature.Species+"_" + );
        Creature creatureComponent = creatureObject.AddComponent<Creature>();
        creatureComponent.Initialize(new_species);
        AddCreature(creatureComponent);
    }
}
