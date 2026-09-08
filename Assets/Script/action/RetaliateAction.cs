using System.Collections.Generic;
using UnityEngine;
using System;
public class RetaliateAction : AttackAction
{
    public override ActionType Type => ActionType.Retaliate;
    public override List<Type> GetAttributeTypes()
    {
        return new List<Type>
        {
            typeof(PredatorIdListAttr)
        };
    }
    public override bool IsConditionMet(Creature creature)
    {
        if (!HurtSystem.UnderAttack(creature.data)) return false;

        if (!creature.data.TryGetAttribute<EnemyAttr>(out var enemyAttr) || enemyAttr.Query() == "") return false;

        if (creature.data.health.Percentage < 0.4f) return false;
        return true;
    }
    public override float GetWeight(Creature creature)
    {
        return 1;
    }
    protected override Creature FindTarget(Creature creature, ActionContext context)
    {
        //為了避免無敵人報錯，但理論上不會
        if (!creature.data.TryGetAttribute<EnemyAttr>(out var enemyAttr) || enemyAttr.Query() == "") return creature;
        Creature enemy = MainManager.inGameManager.Species[creature.data.speciesID].creatures[enemyAttr.Query()];
        return enemy;
    }
    protected override void Attack(Creature creature, Creature target)
    {
        //Debug.Log(creature.creatureBase + " Retaliate!");
        MovementSystem.SetStun(target, 40);
        Vector2 drection = target.transform.position - creature.transform.position;
        HurtSystem.Repeled(target, drection, 50);
        if (!creature.data.TryGetAttribute<EnemyAttr>(out var enemyAttr)) return;
        enemyAttr.Set("");
        HurtSystem.ResetUnderAttackDirection(creature.data);
    }
}