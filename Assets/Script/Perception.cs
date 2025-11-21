using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;

public static class Perception
{
    public static class Creatures
    {
        public static Creature HasTarget(Creature current_creature, int target_ID)
        {
            foreach (var each_species in Manager.Instance.Species)
            {
                if (target_ID != each_species.attributes.species_ID) continue;
                foreach (var each_creature in each_species.creatures)
                {
                    float distance = Vector2.Distance(current_creature.transform.position, each_creature.transform.position);
                    if (distance > current_creature.PerceptionRange) continue;
                    return each_creature;
                }
            }
            return null;
        }
        public static bool HasTarget(Creature creature, List<int> target_ID_list)
        {
            foreach (var target in target_ID_list)
            {
                if (HasTarget(creature, target)) return true;
            }
            return false;
        }

        public static int CountTargetNumber(Creature current_creature, int target_ID)
        {
            int count = 0;
            foreach (var each_species in Manager.Instance.Species)
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

        public static int CountTarget(Creature current_creature, List<int> target_ID_list)
        {
            int count = 0;
            foreach (var target_ID in target_ID_list)
            {
                count += CountTargetNumber(current_creature, target_ID);
            }
            return count;
        }

        public static List<Creature> GetAllTargets(Creature current_creature, int target_ID)
        {
            List<Creature> targetsUUID = new();
            foreach (var each_species in Manager.Instance.Species)
            {
                if (target_ID != each_species.attributes.species_ID) continue;
                foreach (var each_creature in each_species.creatures)
                {
                    float distance = Vector2.Distance(current_creature.transform.position, each_creature.transform.position);
                    if (distance > current_creature.PerceptionRange) continue;
                    targetsUUID.Add(each_creature);
                }
            }
            targetsUUID.Sort();
            return targetsUUID;
        }

        public static List<Creature> GetAllTargets(Creature current_creature, List<int> target_ID_list)
        {
            List<Creature> targets = new();
            foreach (var target_ID in target_ID_list)
            {
                targets.AddRange(GetAllTargets(current_creature, target_ID));
            }
            targets.Sort();
            return targets;
        }
    }

    public class Items
    {
        public static bool HasTarget(Creature creature, FoodType food_type)
        {
            foreach (var each_dropped_item in Manager.FoodItems)
            {
                float distance = Vector2.Distance(creature.transform.position, each_dropped_item.transform.position);
                if (distance > creature.PerceptionRange) continue;
                if (each_dropped_item.Type != food_type) continue;
                return true;
            }
            return false;
        }

        public static bool HasTarget(Creature creature, List<FoodType> food_type_list)
        {
            foreach (var food_type in food_type_list)
            {
                if (HasTarget(creature, food_type)) return true;
            }
            return false;
        }

        public static int CountTargetNumber(Creature creature, FoodType food_type)
        {
            int count = 0;
            foreach (var each_dropped_item in Manager.FoodItems)
            {
                float distance = Vector2.Distance(creature.transform.position, each_dropped_item.transform.position);
                if (distance > creature.PerceptionRange) continue;
                if (each_dropped_item.Type != food_type) continue;
                count++;
            }
            return count;
        }

        public static int CountTarget(Creature creature, List<FoodType> food_type_list)
        {
            int count = 0;
            foreach (var food_type in food_type_list)
            {
                count += CountTargetNumber(creature, food_type);
            }
            return count;
        }

        public static List<Edible> GetAllTargets(Creature creature, FoodType food_type)
        {
            List<Edible> targets = new();
            foreach (var each_dropped_item in Manager.FoodItems)
            {
                float distance = Vector2.Distance(creature.transform.position, each_dropped_item.transform.position);
                if (distance > creature.PerceptionRange) continue;
                if (each_dropped_item.Type != food_type) continue;
                targets.Add(each_dropped_item);
            }
            return targets;
        }

        public static List<Edible> GetAllTargets(Creature creature, List<FoodType> food_type_list)
        {
            List<Edible> targets = new();
            foreach (var food_type in food_type_list)
            {
                targets.AddRange(GetAllTargets(creature, food_type));
            }
            targets.Sort();
            return targets;
        }
    }
}
