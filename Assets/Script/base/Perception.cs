using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using static Perception;

public static class Perception
{
    public static class Creatures
    {
        // Returns the first target creature found within perception range, or null if none found
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
        // Returns true if any target creature from the list is found within perception range
        public static bool HasTarget(Creature creature, List<int> target_ID_list)
        {
            foreach (var target in target_ID_list)
            {
                if (HasTarget(creature, target)) return true;
            }
            return false;
        }
        // Counts the number of target creatures with the specified ID within perception range
        public static int CountTargetNumber(Creature current_creature, int target_ID)
        {
            int count = 0;
            foreach (var each_species in Manager.Instance.Species)
            {
                if (target_ID != each_species.attributes.species_ID) continue;
                foreach (var each_creature in each_species.creatures)
                {
                    if (each_creature == null) continue;
                    float distance = Vector2.Distance(current_creature.transform.position, each_creature.transform.position);
                    if (distance > current_creature.PerceptionRange) continue;
                    count++;
                }
            }
            return count;
        }
        // Counts the total number of target creatures from the list of IDs within perception range
        public static int CountTarget(Creature current_creature, List<int> target_ID_list)
        {
            int count = 0;
            foreach (var target_ID in target_ID_list)
            {
                count += CountTargetNumber(current_creature, target_ID);
            }
            return count;
        }
        // Retrieves a sorted list of all target creatures with the specified ID within perception range
        public static List<Creature> GetAllTargets(Creature current_creature, int target_ID)
        {
            List<Creature> targets = new();
            foreach (var each_species in Manager.Instance.Species)
            {
                if (target_ID != each_species.attributes.species_ID) continue;
                foreach (var each_creature in each_species.creatures)
                {
                    float distance = Vector2.Distance(current_creature.transform.position, each_creature.transform.position);
                    if (distance > current_creature.PerceptionRange) continue;
                    targets.Add(each_creature);
                }
            }
            targets.Sort((x, y) => {
                float distanceX = Vector2.Distance(current_creature.transform.position, x.transform.position);
                float distanceY = Vector2.Distance(current_creature.transform.position, y.transform.position);
                return distanceX.CompareTo(distanceY);
            });
            return targets;
        }
        // Retrieves a sorted list of all target creatures from the list of IDs within perception range
        public static List<Creature> GetAllTargets(Creature current_creature, List<int> target_ID_list)
        {
            List<Creature> targets = new();
            foreach (var target_ID in target_ID_list)
            {
                targets.AddRange(GetAllTargets(current_creature, target_ID));
            }
            targets.Sort((x, y) => {
                float distanceX = Vector2.Distance(current_creature.transform.position, x.transform.position);
                float distanceY = Vector2.Distance(current_creature.transform.position, y.transform.position);
                return distanceX.CompareTo(distanceY);
            });
            return targets;
        }
    }

    public class Items
    {
        // Checks if there is at least one food item of the specified type within perception range
        public static bool HasTarget(Creature creature, FoodType food_type)
        {
            //Debug.Log("HasTarget?");
            foreach (var each_dropped_item in Manager.Instance.FoodItems.Values)
            {
                float distance = Vector2.Distance(creature.transform.position, each_dropped_item.transform.position);
                //Debug.Log("Position: "+each_dropped_item+" ,Distance: "+distance);
                if (distance > creature.PerceptionRange) continue;
                if (each_dropped_item.Type != food_type) continue;
                return true;
            }
            return false;
        }
        // Checks if there is at least one food item from the list of types within perception range
        public static bool HasTarget(Creature creature, List<FoodType> food_type_list)
        {
            //Debug.Log("HasAllKindTarget");
            //if (food_type_list == null) Debug.Log("food type list null");
            foreach (var food_type in food_type_list)
            {
                //Debug.Log("foodType: ");
                if (HasTarget(creature, food_type)) return true;
            }
            return false;
        }
        // Counts the number of food items of the specified type within perception range
        public static int CountTargetNumber(Creature creature, FoodType food_type)
        {
            int count = 0;
            foreach (var each_dropped_item in Manager.Instance.FoodItems.Values)
            {
                float distance = Vector2.Distance(creature.transform.position, each_dropped_item.transform.position);
                if (distance > creature.PerceptionRange) continue;
                if (each_dropped_item.Type != food_type) continue;
                count++;
            }
            return count;
        }
        // Counts the total number of food items from the list of types within perception range
        public static int CountTarget(Creature creature, List<FoodType> food_type_list)
        {
            int count = 0;
            foreach (var food_type in food_type_list)
            {
                count += CountTargetNumber(creature, food_type);
            }
            return count;
        }
        // Retrieves a list of all food items of the specified type within perception range
        public static List<Edible> GetAllTargets(Creature creature, FoodType food_type)
        {
            List<Edible> targets = new();
            foreach (var each_dropped_item in Manager.Instance.FoodItems.Values)
            {
                float distance = Vector2.Distance(creature.transform.position, each_dropped_item.transform.position);
                if (distance > creature.PerceptionRange) continue;
                if (each_dropped_item.Type != food_type) continue;
                targets.Add(each_dropped_item);
            }
            return targets;
        }
        // Retrieves a sorted list of all food items from the list of types within perception range
        public static List<Edible> GetAllTargets(Creature creature, List<FoodType> food_type_list)
        {
            List<Edible> targets = new();
            foreach (var food_type in food_type_list)
            {
                targets.AddRange(GetAllTargets(creature, food_type));
            }
            targets.Sort((x, y) => {
                float distanceX = Vector2.Distance(creature.transform.position, x.transform.position);
                float distanceY = Vector2.Distance(creature.transform.position, y.transform.position);
                return distanceX.CompareTo(distanceY);
            });
            return targets;
        }
    }
}
