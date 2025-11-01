using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

public static class Perception
{
    public static Creature HasTarget(Creature current_creature, int target_ID)
    {
        foreach (var each_species in Manager.species)
        {
            if(target_ID!=each_species.attributes.species_ID) continue;
            foreach(var each_creature in each_species.creatures){
                float distance = Vector2.Distance(current_creature.transform.position, each_creature.transform.position);
                if (distance > current_creature.PerceptionRange) continue;
                return each_creature;
            }
        }
        return null;
    }
    public static int CountTargetNumber(Creature current_creature, int target_ID)
    {
        int count = 0;
        foreach (var each_species in Manager.species)
        {
            if (target_ID != each_species.attributes.species_ID) continue;
            foreach (var each_creature in each_species.creatures)
            {
                float distance = Vector2.Distance(current_creature.transform.position, each_creature.transform.position);
                if (distance > current_creature.PerceptionRange) continue;
                count++;
            }
        }
        return count;
    } 
    public static bool HasTarget(Creature creature, List<int> target_ID_list)
    {
        foreach (var target in target_ID_list)
        {
            if (HasTarget(creature, target)) return true;
        }
        return false;
    }
}
