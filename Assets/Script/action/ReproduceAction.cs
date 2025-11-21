using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.Image;

public class ReproduceAction : ActionBase
{
    public ReproduceAction() { }

    public override ActionType Type => ActionType.Reproduce;
    public override int Cooldown => 500;

    public override bool IsConditionMet(Creature creature)
    {
        //return true;
        if (creature.Age < creature.Lifespan * 0.2f) return false;    // 年齡未達20%
        //if (creature.Hunger <= creature.MaxHunger * 0.5f) return false; // 飢餓值低於50%
        if (creature.ReproductionCooldown > 0) return false; // 繁殖冷卻中
        //if (Perception.HasTarget(creature, creature.PredatorIDList)) return false; // 附近有敵人
        return true;
    }

    public override float GetWeight(Creature creature)
    {
        return 1;
        //(1 / 偵查範圍內同類個體數 + 1) * 0.8
        //return (1f / (Perception.CountTargetNumber(creature,creature.SpeciesID)+1)) * 0.8f;
    }

    public override bool IsSuccess(Creature creature)
    {
        return true;
        //return Random.value < 0.6f; // 60% 成功率
    }

    public override void Execute(Creature creature)
    {
        Debug.Log("ReproduceAction");
        int creature_num=0;
        Debug.Log("Manager.Instance = " + (Manager.Instance == null));
        Debug.Log("Manager.Instance.Species = " + (Manager.Instance?.Species == null));
        Manager.Instance.RegisterCreature(creature);
        foreach (var each_species in Manager.Instance.Species)
        {
            if(each_species.attributes.species_ID == creature.SpeciesID)
            {
                creature_num=each_species.creatures.Count-1;
            }
        }
        GameObject new_game_object = UnityEngine.Object.Instantiate(creature.gameObject);
        new_game_object.name = "creature " + creature.SpeciesID + "." + creature_num;
        Creature new_creature = new_game_object.GetComponent<Creature>();
        new_creature.Initialize(creature.ToCreatureAttribute(), new_game_object);
    }
}
