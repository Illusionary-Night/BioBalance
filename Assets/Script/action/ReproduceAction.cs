using UnityEngine;
using static Perception;
//TODO：有點忘了這裡為啥不用RCD
public class ReproduceAction : ActionBase
{
    public ReproduceAction() { }

    public override ActionType Type => ActionType.Reproduce;

    public override bool IsConditionMet(Creature creature)
    {
        if (creature.data.age.Percentage < 0.2f) return false;
        if (creature.data.hunger.Percentage < 0.5f) return false;
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
        int kindredCount = Perception.Creatures.CountTargetNumber(creature, creature.data.speciesID);
        float crowdPressure = Mathf.Pow(kindredCount / crowdThreshold, steepness);
        float finalChance = maxChance * (1f - Mathf.Clamp01(crowdPressure));

        // 隨機判定
        return UnityEngine.Random.value < finalChance;
        //return true;
    }

    public override void Execute(Creature creature, ActionContext context = null)
    {
        // 使用物件池取得新生物
        Vector3 spawnPosition = creature.transform.position + new Vector3(Random.value % 100 / 100f, Random.value % 100 / 100f, 0);
        Creature new_creature = CreaturePool.GetCreature(creature.data.species, spawnPosition, creature.ToCreatureAttribute());
        if (new_creature == null)
        {
            Debug.LogWarning("Failed to spawn new creature because the pool is exhausted.");
            return;
        }
        new_creature.gameObject.name = new_creature.data.creatureBase + "_" + new_creature.data.UUID;
        MainManager.inGameManager.RegisterCreature(new_creature);

        // 繁殖是立即完成的 Action
        context?.Complete();
    }
}
