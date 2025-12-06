using System.Collections.Generic;
using UnityEngine;

class EatAction : ActionBase
{
    public override ActionType Type => ActionType.Eat;
    public override int Cooldown => 10;
    
    public override bool IsConditionMet(Creature creature)
    {
        return Perception.Items.HasTarget(creature, creature.FoodTypes);
    }
    
    public override float GetWeight(Creature creature)
    {
        return 1;
    }
    
    public override bool IsSuccess(Creature creature)
    {
        return Random.Range(0,3)==0;
    }
    
    public override void Execute(Creature creature, ActionContext context = null)
    {
        List<Edible> edibleTargets = Perception.Items.GetAllTargets(creature, creature.FoodTypes);
        if (edibleTargets.Count > 0)
        {
            Edible food = edibleTargets[0];
            Vector2Int foodPosition = Vector2Int.RoundToInt(food.transform.position);
            
            // 使用狀態機註冊移動回調
            var stateMachine = creature.GetStateMachine();
            
            System.Action<Vector2Int> onArrived = (arrivedPosition) =>
            {
                // 檢查 Context 是否仍然有效
                if (context != null && !context.IsValid)
                {
                    return;
                }
                
                // 確認到達的是目標位置
                if (Vector2.Distance(arrivedPosition,foodPosition)<1.5f)
                {
                    // 檢查食物是否仍然存在
                    if (food != null)
                    {
                        Debug.LogWarning("eat");
                        food.Eaten();
                        creature.Hunger = Mathf.Min(creature.Hunger + food.NutritionalValue, creature.MaxHunger);
                    }
                    else
                    {
                        Debug.LogWarning("food is null");
                    }
                    
                    // 標記 Action 完成
                    context?.Complete();
                }
            };
            
            // 透過狀態機註冊回調（自動管理清理）
            stateMachine.RegisterMovementCallback(onArrived);
            creature.MoveTo(foodPosition);
        }
        else
        {
            // 沒有找到食物，直接標記為完成
            context?.Complete();
        }
        creature.ActionCooldown = Cooldown;
    }
}