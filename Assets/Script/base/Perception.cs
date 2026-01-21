using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.GraphicsBuffer;
using static Perception;
using System.Linq;

public static class Perception
{
    public static class Creatures
    {
        // Returns the first target creature found within perception range, or null if none found
        public static bool HasTarget(Creature current_creature, int target_ID)
        {
            if (!Manager.Instance.Species.TryGetValue(target_ID, out var target_species))
                return false;

            float range = current_creature.perceptionRange;
            float rangeSq = range * range; // 預先算好範圍的平方

            return target_species.creatures.Values.Any(c =>
                c != null && !c.IsDead && c != current_creature &&
                (current_creature.transform.position - c.transform.position).sqrMagnitude < rangeSq
            );
        }
        // Returns true if any target creature from the list is found within perception range
        public static bool HasTarget(Creature creature, List<int> target_ID_list)
        {
            return target_ID_list?.Any(id => HasTarget(creature, id))??false;
        }
        // Counts the number of target creatures with the specified ID within perception range
        public static int CountTargetNumber(Creature current_creature, int target_ID)
        {
            if (!Manager.Instance.Species.TryGetValue(target_ID, out var target_species))
                return 0;

            float range = current_creature.perceptionRange;
            float rangeSq = range * range; // 預先算好範圍的平方

            return target_species.creatures.Values.Count(c =>
                c != null && !c.IsDead && c != current_creature &&
                (current_creature.transform.position - c.transform.position).sqrMagnitude < rangeSq
            );
        }
        // Counts the total number of target creatures from the list of IDs within perception range
        public static int CountTarget(Creature current_creature, List<int> target_ID_list)
        {
            return target_ID_list?.Sum(id => CountTargetNumber(current_creature,id)) ?? 0;
        }
        // Retrieves a sorted list of all target creatures with the specified ID within perception range
        public static List<Creature> GetAllTargets(Creature current_creature, int target_ID)
        {
            if (!Manager.Instance.Species.TryGetValue(target_ID, out var target_species))
                return new List<Creature>();

            Vector2 currentPos = current_creature.transform.position;
            float range = current_creature.perceptionRange;
            float rangeSq = range * range; // 預算平方以優化效能

            return target_species.creatures.Values
                .Where(c => c != null && !c.IsDead && c != current_creature) // 過濾無效目標
                .Where(c => (currentPos - (Vector2)c.transform.position).sqrMagnitude < rangeSq) // 範圍判定
                .OrderBy(c => (currentPos - (Vector2)c.transform.position).sqrMagnitude) // 由近到遠排序
                .ToList(); // 轉回 List
        }
        // Retrieves a sorted list of all target creatures from the list of IDs within perception range
        public static List<Creature> GetAllTargets(Creature current_creature, List<int> target_ID_list)
        {
            Vector2 currentPos = current_creature.transform.position;

            return target_ID_list?
                .SelectMany(id => GetAllTargets(current_creature, id)) // 將多個 List 合併為一個序列
                .OrderBy(c => ((Vector2)c.transform.position - currentPos).sqrMagnitude) // 統一進行距離排序
                .ToList() ?? new List<Creature>(); // 確保不回傳 null
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
                if (distance > creature.perceptionRange) continue;
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
                if (distance > creature.perceptionRange) continue;
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
                if (distance > creature.perceptionRange) continue;
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
