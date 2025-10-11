using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public static class Perception
{
    public static bool HasTarget(Creature creature, CreatureAttributes target)
    {
        if (creature == null) return false; 
        if (target.species == null) return false;
        foreach (var c in Manager.creatures)
        {
            if(!target.species.Equals(c.Species)) continue; //不是目標物種
            if (Vector2.Distance(creature.transform.position, c.transform.position) > creature.PerceptionRange) continue;   //超出感知範圍
            return true;
        }
        return false;
    }
    public static int CountTargetNumber(Creature creature, CreatureAttributes target)
    {
        int count = 0;
        if (creature == null) return 0;
        if (target.species == null) return 0;
        foreach (var c in Manager.creatures)
        {
            if (!target.species.Equals(c.Species)) continue; //不是目標物種
            if (Vector2.Distance(creature.transform.position, c.transform.position) > creature.PerceptionRange) continue;   //超出感知範圍
            count++;
        }
        return count;
    } 
    public static bool HasTarget(Creature creature, List<CreatureAttributes> targetList)
    {
        foreach (var target in targetList)
        {
            if (HasTarget(creature, target)) return true;
        }
        return false;
    }
}
