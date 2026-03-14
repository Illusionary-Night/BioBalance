using UnityEngine;
using UnityEngine.Analytics;
using static Perception;

public class MatingAction : ActionBase
{
    public override ActionType Type => ActionType.Mating;
    
    private float stopDistance = 0.5f;

    public override bool IsConditionMet(Creature creature)
    {
        if (Perception.Creatures.HasTarget(creature, creature.speciesID)) return false;
        if (creature.age < creature.lifespan * 0.2f) return false;
        if (creature.hunger < creature.maxHunger * 0.5f) return false;
        if (creature.gender == Gender.Female) {
            if(creature.reproductionCD > 0)return false;
        }
        return true;
    }

    public override float GetWeight(Creature creature)
    {
        // --- 雄性：舔狗模式 ---
        if (creature.gender == Gender.Male)
        {
            // 如果太餓，先去吃飯，不當舔狗
            if (creature.hunger < creature.maxHunger * 0.4f) return 0f;

            // 只要周邊有可以受孕的雌性，就維持追逐興趣-----------------------------------------wait to slove
            if (true)
            {
                return 0.65f; // 比漫遊高，比吃飯低
            }
        }

        // --- 雌性：女王模式 ---
        if (creature.gender == Gender.Female)
        {
            // 1. 先看自己準不準備好 (冷卻、飽食度)----------------------------------------wait to slove
            //if () return 0f;

            // 2. 看看身邊有沒有已經貼過來的雄性 (範圍要小，例如 1.0f)
            //bool maleNearby = Perception.HasMaleNearby(creature, 1.0f);

            //if (maleNearby)
            //{
            //    // 萬事具備，權重拉高，準備執行 Execute (交配)
            //    return 0.85f;
            //}
        }

        return 0f;
    }

    public override bool IsSuccess(Creature creature)
    {
        return Random.Range(0, 9) < 9;
    }

    public override void Execute(Creature creature, ActionContext context)
    {
        // 1. 如果還沒有伴侶，先找一個
        if (creature.matingPartner == null)
        {
            // 使用較大的感應倍率 (2.0x) 尋找同類，不需要排序，我們手動篩選
            var potentialPartners = Perception.Creatures.GetAllTargets(creature, creature.speciesID, 2.0f, false);

            foreach (var p in potentialPartners)
            {
                // 條件：異性 + 也準備好了 + 沒伴侶 (或是伴侶剛好就是我)
                if (p.gender != creature.gender)//----------------------------------------wait to slove
                {
                    if (p.matingPartner == null || p.matingPartner == creature)
                    {
                        // 雙向鎖定
                        creature.matingPartner = p;
                        p.matingPartner = creature;
                        break;
                    }
                }
            }
        }

        // 2. 如果還是沒找到，此動作失敗
        if (creature.matingPartner == null)
        {
            context.Complete();
            return;
        }

        // 3. 執行移動：雙向奔赴 
        creature.MoveTo(creature.matingPartner.GetRoundedPosition(), isRunning: false);
    }

    //public override void OnTick(Creature creature, ActionContext context)
    //{
    //    // 安全檢查：如果伴侶死掉或跑遠了或不想生了，就重置
    //    if (creature.matingPartner == null || creature.matingPartner.isDead || !creature.matingPartner.IsReadyToMate())
    //    {
    //        creature.matingPartner = null;
    //        context.Complete();
    //        return;
    //    }

    //    float distSq = (creature.transform.position - creature.matingPartner.transform.position).sqrMagnitude;

    //    // 4. 判斷是否接觸
    //    if (distSq < stopDistance * stopDistance)
    //    {
    //        // 為了避免重複產子，由 Female 負責 Spawn
    //        if (creature.gender == Gender.Female)
    //        {
    //            GiveBirth(creature, creature.matingPartner);
    //        }

    //        // 任務完成，重置狀態
    //        creature.reproductionUrge = 0;
    //        creature.matingPartner = null;
    //        context.Complete();
    //    }
    //    else
    //    {
    //        // 持續更新伴侶位置 (滾動式目標)
    //        creature.MoveTo(creature.matingPartner.GetRoundedPosition(), isRunning: true);
    //    }
    //}

    private void GiveBirth(Creature mother, Creature father)
    {
        // 使用物件池取得新生物
        Vector3 spawnPosition = mother.transform.position + new Vector3(Random.value % 100 / 100f, Random.value % 100 / 100f, 0);
        Creature baby = CreaturePool.GetCreature(mother.mySpecies, mother.ToCreatureAttribute(), spawnPosition);
        baby.gameObject.name = baby.creatureBase + "_" + baby.UUID;
        Manager.Instance.RegisterCreature(baby);
        if (baby != null)
        {
            // 2. 紀錄父母的 ID
            baby.SetFatherID(father.UUID);
            baby.SetMotherID(mother.UUID);

        }
    }
}
