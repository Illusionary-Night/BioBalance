using System.Collections.Generic;
using UnityEngine;

public class RetaliateAction : AttackAction
{
    public override ActionType Type => ActionType.Retaliate;
    public override bool IsConditionMet(Creature creature)
    {
        if (!creature.UnderAttack())return false;
        if (creature.enemy == null)return false;
        if(creature.health < creature.maxHealth * 0.4f)return false;
        return true;
    }
    public override float GetWeight(Creature creature)
    {
        return 1;
    }
    protected override Creature FindTarget(Creature creature, ActionContext context)
    {
        return creature.enemy;
    }
    protected override void Attack(Creature creature, Creature target)
    {
        Debug.Log(creature.creatureBase + " Retaliate!");
        target.SetStun(40);
        Vector2 drection = target.transform.position - creature.transform.position;
        target.Repel(drection, 50);
        creature.SetEnemy(null);
    }
}