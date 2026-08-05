using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;

public class MatingAction : ActionBase
{
    public override ActionType Type => ActionType.Mating;

    public override bool IsConditionMet(Creature creature)
    {
        // --- 1. 基本生理限制 (男女通用) ---
        if (creature.data.age.Percentage < 0.2f) return false;     // 未成年不交配
        if (creature.data.hunger.Percentage < 0.5f) return false; // 肚子餓不交配
        if (creature.reproductionCD > 0) return false;                 // 冷卻中不交配

        // --- 2. 確保視線內有「合法」的對象 ---
        if (creature.gender == Gender.Female)
        {
            // 母找公：對方也必須是成年、吃飽、且不在冷卻中
            bool maleInSight = Perception.Creatures.HasTarget(creature, creature.speciesID, 1.5f,
                c => c.gender == Gender.Male &&
                     c.data.age.Percentage >= 0.2f &&
                     c.data.hunger.Percentage >= 0.5f);

            if (!maleInSight) return false;
        }
        else if (creature.gender == Gender.Male)
        {
            // 公找母：對方也必須是成年、吃飽、且不在冷卻中
            bool femaleInSight = Perception.Creatures.HasTarget(creature, creature.speciesID, 1.5f,
                c => c.gender == Gender.Female &&
                     c.data.age.Percentage >= 0.2f &&
                     c.data.hunger.Percentage >= 0.5f);

            if (!femaleInSight) return false;
        }

        return true;
    }
    public override float GetWeight(Creature creature)
    {
        // --- 雄性：舔狗模式 ---
        if (creature.gender == Gender.Male)
        {

            // 只要周邊有可以受孕的雌性，就維持追逐興趣
            if (Perception.Creatures.HasTarget(creature, creature.speciesID, 1, c => c.IsInHeat()))
            {
                return 0.65f; // 比漫遊高，比吃飯低
            }
        }

        // --- 雌性：女王模式 ---
        if (creature.gender == Gender.Female)
        {
            // 1. 先看自己準不準備好 (冷卻、飽食度)
            if (!creature.IsInHeat()) return 0f;

            // 2. 看看身邊有沒有已經貼過來的雄性 (範圍要小，例如 1.0f)
            bool maleNearby = Perception.Creatures.HasTarget(creature, creature.speciesID, 1.5f, c => c.IsNearby(creature) && c.gender == Gender.Male && c.IsInHeat());

            if (maleNearby)
            {
                // 萬事具備，權重拉高，準備執行 Execute (交配)
                //Debug.LogAssertion("success2");
                return 0.85f;
            }
        }

        return 0f;
    }

    public override bool IsSuccess(Creature creature)
    {
        return Random.Range(0, 9) < 9;
    }

    public override void Execute(Creature creature, ActionContext context)
    {
        if (creature.gender == Gender.Male)
        {
            //衝去找雌性
            List<Creature> optionalTargets = Perception.Creatures.GetAllTargets(creature, creature.speciesID, 1, true, c => c.gender == Gender.Female && c.IsInHeat());
            Creature target = optionalTargets.FirstOrDefault();
            if (target != null)
            {
                Vector2Int targetPosition = Vector2Int.RoundToInt(target.transform.position);
                Collider2D targetCollider = target.GetComponent<Collider2D>();
                if (targetCollider == null)
                {
                    Debug.LogWarning("collider missing");
                    return;
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
                    // 確認是否在附近
                    if (target != null && creature.IsNearby(target))
                    {
                        // 標記 Action 完成
                        //Debug.LogAssertion("mating arrive");
                        context?.Complete();
                    }
                };

                // 透過狀態機註冊回調（自動管理清理）
                stateMachine.RegisterMovementCallback(onArrived);
                creature.MoveTo(targetPosition, false);
            }
            else
            {
                // 沒有找到目標，直接標記為完成
                context?.Complete();
            }
        }
        if (creature.gender == Gender.Female)
        {
            //選老公
            List<Creature> optionalTargets = Perception.Creatures.GetAllTargets(creature, creature.speciesID, 1, true, c => c.IsNearby(creature) && c.gender == Gender.Male && c.IsInHeat());
            Creature target = optionalTargets.FirstOrDefault();
            if (target != null)
            {
                Vector2Int targetPosition = Vector2Int.RoundToInt(target.transform.position);
                int times = CalculateBirthCount(creature.reproductionRate);
                for (int i = 0; i < times; i++)
                {
                    GiveBirth(creature, target);
                    creature.reproductionCD = 100f;
                    target.reproductionCD = 30f;
                }
                creature.reproductionCD = 100f;
                context?.Complete();
            }
            else
            {
                // 沒有找到目標，直接標記為完成
                //Debug.LogAssertion("dont fine male");
                context?.Complete();
            }
        }
    }
    //TODO: 之後Builder會把出生整合在一起
    private void GiveBirth(Creature mother, Creature father)
    {
        Species species = MainManager.inGameManager.Species[mother.speciesID];
        if (species.creatures.Count >= 300)
        {
            return;
        }
        //Debug.LogAssertion("mating success!");
        // 使用物件池取得新生物
        Vector3 spawnPosition = mother.transform.position + (Vector3)(Random.insideUnitCircle * 0.5f);
        Creature baby = CreaturePool.GetCreature(mother.mySpecies, spawnPosition, mother.ToCreatureAttribute(), father.ToCreatureAttribute());
        if (baby == null)
        {
            Debug.LogWarning("Failed to spawn baby creature because the pool is exhausted.");
            return;
        }
        baby.gameObject.name = baby.creatureBase + "_" + baby.UUID;
        MainManager.inGameManager.RegisterCreature(baby);
        if (baby != null)
        {
            // 2. 紀錄父母的 ID
            baby.SetFatherID(father.UUID);
            baby.SetMotherID(mother.UUID);

        }
    }

    public int CalculateBirthCount(float rate)
    {
        // 取出整數部分 (例如 2.4 -> 2)
        int baseCount = Mathf.FloorToInt(rate);

        // 取出小數部分 (例如 2.4 - 2 = 0.4)
        float fraction = rate - baseCount;

        // 擲骰子決定是否因為小數點而多生一隻
        if (UnityEngine.Random.value < fraction)
        {
            baseCount++;
        }

        return baseCount;
    }
}
