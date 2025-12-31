using UnityEngine;

public class ReproduceAction : ActionBase
{
    public ReproduceAction() { }

    public override ActionType Type => ActionType.Reproduce;
    //public override int Cooldown => 500;

    public override bool IsConditionMet(Creature creature)
    {
        if (creature.Age < creature.Lifespan * 0.2f) return false;
        //if (creature.ReproductionCooldown > 0) return false;
        return true;
    }

    public override float GetWeight(Creature creature)
    {
        return 0.5f;
    }

    public override bool IsSuccess(Creature creature)
    {
        return Random.Range(0, 3) == 0;
    }

    public override void Execute(Creature creature, ActionContext context = null)
    {
        int creature_num = 0;
        Manager.Instance.RegisterCreature(creature);
        creature.MoveTo(creature.GetRoundedPosition());   
        foreach (var each_species in Manager.Instance.Species)
        {
            if (each_species.attributes.species_ID == creature.SpeciesID)
            {
                creature_num = each_species.creatures.Count - 1;
            }
        }
        
        GameObject new_game_object = UnityEngine.Object.Instantiate(creature.gameObject);
        new_game_object.name = "creature " + creature.SpeciesID + "." + creature_num;
        Creature new_creature = new_game_object.GetComponent<Creature>();
        new_creature.Initialize(creature.ToCreatureAttribute(), new_game_object);
        
        //creature.ReproductionCooldown = creature.ReproductionInterval;

        // 繁殖是立即完成的 Action
        context?.Complete();
    }
}
