using UnityEngine;
using static Perception;

public class ReproduceAction : ActionBase
{
    public ReproduceAction() { }

    public override ActionType Type => ActionType.Reproduce;
    //public override int Cooldown => 500;

    public override bool IsConditionMet(Creature creature)
    {
        if (creature.Age < creature.Lifespan * 0.2f) return false;
        if (creature.Hunger < creature.MaxHunger * 0.5f) return false;
        return true;
    }

    public override float GetWeight(Creature creature)
    {
        return 0.5f;
    }

    public override bool IsSuccess(Creature creature)
    {
        // --- 參數設定 ---
        float maxChance = 0.7f;       // 附近完全沒人時，最大的繁殖機率 (40%)
        float crowdThreshold = 8f;    // 當附近達到 8 隻時，成功率降為幾乎為 0
        float steepness = 2f;         // 曲線陡峭度（數值越高，對擁擠越敏感）

        // 核心公式：使用 Sigmoid 的變體或簡單的線性下降
        // 這裡使用 1 / (1 + e^(N-T)) 的邏輯簡化版
        int kindredCount = Perception.Creatures.CountTargetNumber(creature, creature.SpeciesID);
        float crowdPressure = Mathf.Pow(kindredCount / crowdThreshold, steepness);
        float finalChance = maxChance * (1f - Mathf.Clamp01(crowdPressure));

        // 隨機判定
        return UnityEngine.Random.value < finalChance;
        //return true;
    }

    public override void Execute(Creature creature, ActionContext context = null)
    {
        int creature_num = 0;
        //Manager.Instance.RegisterCreature(creature);
        creature.MoveTo(creature.GetRoundedPosition());
        foreach (var each_species in Manager.Instance.Species)
        {
            if (each_species.attributes.species_ID == creature.SpeciesID)
            {
                creature_num = each_species.creatures.Count - 1;
            }
        }

        // 使用物件池取得新生物
        Vector3 spawnPosition = creature.transform.position + new Vector3(Random.value % 100 / 100f, Random.value % 100 / 100f, 0);
        Creature new_creature = CreaturePool.GetCreature(creature.ToCreatureAttribute(), spawnPosition);
        new_creature.gameObject.name = "creature_" + new_creature.SpeciesID + "_" + new_creature.UUID;
        Manager.Instance.RegisterCreature(new_creature);

        // 繁殖是立即完成的 Action
        context?.Complete();
    }
}
