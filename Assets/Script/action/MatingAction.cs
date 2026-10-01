using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Analytics;
using System;
//TODO: reproductionCD 和 gender需要處理，RCD是屬於action cd，gender的部分(？
public class MatingAction : ActionBase
{
    public override ActionType Type => ActionType.Mating;
    public override List<Type> GetAttributeTypes()
    {
        return new List<Type>
        {
            typeof(ReproductionCdAttr),
            typeof(GenderAttr),
            typeof(MotherIdAttr),
            typeof(FatherIdAttr)
        };
    }
    public override bool IsConditionMet(Creature creature)
    {
        if (!IsReadyToMate(creature)) return false;

        var genderAttr = creature.data.GetAttribute<GenderAttr>();
        if (genderAttr == null) return false; // 無性別生物不參與此 Action

        // --- 確保視線內有「合法」的異性 ---
        Gender targetGender = genderAttr.Query() == Gender.Male ? Gender.Female : Gender.Male;

        bool mateInSight = Perception.Creatures.HasTarget(creature, creature.data.speciesID, 1.5f,
            c =>
            {
                var targetGenderAttr = c.data.GetAttribute<GenderAttr>();
                return targetGenderAttr != null
                    && targetGenderAttr.Query() == targetGender
                    && IsReadyToMate(c); // 對方也必須準備好
            });

        return mateInSight;
    }
    public override float GetWeight(Creature creature)
    {
        if (!IsReadyToMate(creature)) return 0;

        var genderAttr = creature.data.GetAttribute<GenderAttr>();
        if (genderAttr == null) return 0; // 無性別生物不參與此 Action

        // --- 雄性：舔狗模式 ---
        if (genderAttr.Query() == Gender.Male)
        {
            // 只要周邊有可以受孕的雌性，就維持追逐興趣
            if (Perception.Creatures.HasTarget(creature, creature.data.speciesID, 1, c =>
            {
                var targetGender = c.data.GetAttribute<GenderAttr>();
                return targetGender != null && targetGender.Query() == Gender.Female && IsReadyToMate(c);
            }))
            {
                return 0.65f; // 比漫遊高，比吃飯低
            }
        }

        // --- 雌性：女王模式 ---
        if (genderAttr.Query() == Gender.Female)
        {
            // 看看身邊有沒有已經貼過來的雄性
            bool maleNearby = Perception.Creatures.HasTarget(creature, creature.data.speciesID, 1.5f, c =>
            {
                var targetGender = c.data.GetAttribute<GenderAttr>();
                return IsNearby(creature, c) && targetGender != null && targetGender.Query() == Gender.Male && IsReadyToMate(c);
            });

            if (maleNearby)
            {
                return 0.85f;
            }
        }

        return 0f;
    }
    public override bool IsSuccess(Creature creature)
    {
        return UnityEngine.Random.Range(0, 9) < 9;
    }
    public override void Execute(Creature creature, ActionContext context)
    {
        var genderAttr = creature.data.GetAttribute<GenderAttr>();
        if (genderAttr == null)
        {
            context?.Complete();
            return;
        }

        if (genderAttr.Query() == Gender.Male)
        {
            //衝去找雌性
            List<Creature> optionalTargets = Perception.Creatures.GetAllTargets(creature, creature.data.speciesID, 1, true, c =>
            {
                var targetGender = c.data.GetAttribute<GenderAttr>();
                return targetGender != null && targetGender.Query() == Gender.Female && IsReadyToMate(c);
            });

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
                var stateMachine = creature.data.actionStateMachine;

                System.Action<Vector2Int> onArrived = (arrivedPosition) =>
                {
                    // 檢查 Context 是否仍然有效
                    if (context != null && !context.IsValid)
                    {
                        return;
                    }
                    // 確認是否在附近 (改用你寫好的 IsNearby 方法)
                    if (target != null && IsNearby(creature, target))
                    {
                        // 標記 Action 完成
                        context?.Complete();
                    }
                };

                // 透過狀態機註冊回調（自動管理清理）
                stateMachine.RegisterMovementCallback(onArrived);
                MovementSystem.MoveTo(creature, targetPosition, false);
            }
            else
            {
                // 沒有找到目標，直接標記為完成
                context?.Complete();
            }
        }
        else if (genderAttr.Query() == Gender.Female)
        {
            //選老公
            List<Creature> optionalTargets = Perception.Creatures.GetAllTargets(creature, creature.data.speciesID, 1, true, c =>
            {
                var targetGender = c.data.GetAttribute<GenderAttr>();
                return IsNearby(creature, c) && targetGender != null && targetGender.Query() == Gender.Male && IsReadyToMate(c);
            });

            Creature target = optionalTargets.FirstOrDefault();

            if (target != null)
            {
                // 修正：從 data 取得繁殖率
                int times = CalculateBirthCount(creature.data.reproductionRate);

                for (int i = 0; i < times; i++)
                {
                    GiveBirth(creature, target);
                }

                // 修正：透過 Attribute 寫入冷卻時間
                // 註：我這邊先寫 .Value，如果你們修改數值的方法是 .Set(100f) 或 .SetBaseValue(100f) 請自行替換一下！
                var motherCD = creature.data.GetAttribute<ReproductionCdAttr>();
                if (motherCD != null) motherCD.SetBaseValue(100f);

                var fatherCD = target.data.GetAttribute<ReproductionCdAttr>();
                if (fatherCD != null) fatherCD.SetBaseValue(30f);

                context?.Complete();
            }
            else
            {
                // 沒有找到目標，直接標記為完成
                context?.Complete();
            }
        }
    }
    private void GiveBirth(Creature mother, Creature father)
    {
        Species species = MainManager.inGameManager.Species[mother.data.speciesID];
        if (species.creatures.Count >= 300)
        {
            return;
        }
        //Debug.LogAssertion("mating success!");
        // 使用物件池取得新生物
        Vector3 spawnPosition = mother.transform.position + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.5f);
        Creature baby = CreatureBuilder.Generate(mother.data.species, spawnPosition, null, mother.data, father.data);
        if (baby == null)
        {
            Debug.LogWarning("Failed to spawn baby creature because the pool is exhausted.");
            return;
        }
        baby.gameObject.name = baby.data.creatureBase + "_" + baby.data.UUID;
        MainManager.inGameManager.RegisterCreature(baby);
        if (baby != null)
        {
            baby.data.GetAttribute<FatherIdAttr>().Set(father.data.UUID);
            baby.data.GetAttribute<FatherIdAttr>().Set(mother.data.UUID);
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
    private bool IsReadyToMate(Creature c)
    {
        if (c.data.isDead) return false;
        if (c.data.age.Percentage < 0.2f) return false;      // 未成年不交配
        if (c.data.hunger.Percentage < 0.5f) return false;   // 肚子餓不交配

        // 檢查冷卻時間
        var cdAttr = c.data.GetAttribute<ReproductionCdAttr>();
        if (cdAttr != null && cdAttr.Value > 0) return false; // 冷卻中不交配

        return true;
    }
    private bool IsNearby(Creature creature, Creature target)
    {
        // TODO: 這邊要重新定義一次鄰近，也可能可以寫在Perception
        return false;
    }
}
