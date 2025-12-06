using System.Collections.Generic;
using UnityEngine;

class EatAction : ActionBase
{
    public override ActionType Type => ActionType.Eat;
    public override int Cooldown => 300;
    public override bool IsConditionMet(Creature creature)
    {
        Debug.Log("has target: "+Perception.Items.HasTarget(creature, creature.FoodTypes));
        // TODO: 檢查是否有可食用的目標
        return Perception.Items.HasTarget(creature, creature.FoodTypes);
    }
    public override float GetWeight(Creature creature)
    {
        return 1;
        // 根據飢餓值決定進食權重，飢餓值越高，進食權重越高
        //return 1 - creature.Hunger / creature.MaxHunger;
    }
    public override bool IsSuccess(Creature creature)
    {
        return true;
        // 80% 機率成功進食
        //return Random.Range(0, 10) < 8;
    }
    public override void Execute(Creature creature)
    {
        Debug.Log("Eat");
        List<Edible> edibleTargets = Perception.Items.GetAllTargets(creature, creature.FoodTypes);
        if (edibleTargets.Count > 0)
        {
            Edible food = edibleTargets[0];
            //TODO: 走過去
            creature.MoveTo(Vector2Int.RoundToInt(food.transform.position));
            //TODO: Corutine 等待走過去
            food.Eaten();
            creature.Hunger = Mathf.Min(creature.Hunger + food.NutritionalValue, creature.MaxHunger);
        }
    }
}