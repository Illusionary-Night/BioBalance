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
        public static bool HasTarget(Creature current_creature, int target_ID, float rangeMultiplier = 1.0f)
        {
            // 安全檢查：發起偵測的生物必須存在且存活
            if (current_creature == null || current_creature.isDead)
                return false;

            // 檢查目標物種是否存在於 Manager 中
            if (!MainManager.inGameManager.Species.TryGetValue(target_ID, out var target_species))
                return false;

            // 使用 Max(0) 因為有時我們會需要 > 1.0 的加成（如：警戒狀態 150% 視野）
            float finalMultiplier = Mathf.Max(0f, rangeMultiplier);
            float range = current_creature.perceptionRange * finalMultiplier;
            float rangeSq = range * range;

            // 執行搜尋與過濾
            return target_species.creatures.Values.Any(c =>
                c != null &&
                !c.isDead &&
                c != current_creature && // 不會偵測到自己
                (current_creature.transform.position - c.transform.position).sqrMagnitude <= rangeSq
            );
        }
        // Returns true if any target creature from the list is found within perception range
        public static bool HasTarget(Creature creature, List<int> target_ID_list, float rangeMultiplier = 1.0f)
        {
            // 安全檢查：發起者必須存在
            if (creature == null || creature.isDead) return false;

            // 列表檢查：若清單為 null 或長度為 0，直接回傳 false
            if (target_ID_list == null || target_ID_list.Count == 0) return false;

            return target_ID_list.Any(id => HasTarget(creature, id, rangeMultiplier));
        }
        // Counts the number of target creatures with the specified ID within perception range
        public static int CountTargetNumber(Creature current_creature, int target_ID, float rangeMultiplier = 1.0f)
        {
            // 安全檢查：發起偵測的生物必須存在且存活
            if (current_creature == null || current_creature.isDead)
                return 0;

            // 檢查目標物種是否存在於 Manager 中
            if (!MainManager.inGameManager.Species.TryGetValue(target_ID, out var target_species))
                return 0;

            float finalMultiplier = Mathf.Max(0f, rangeMultiplier);
            float range = current_creature.perceptionRange * finalMultiplier;
            float rangeSq = range * range; 

            return target_species.creatures.Values.Count(c =>
                c != null && !c.isDead && c != current_creature &&
                (current_creature.transform.position - c.transform.position).sqrMagnitude < rangeSq
            );
        }
        // Counts the total number of target creatures from the list of IDs within perception range
        public static int CountTarget(Creature current_creature, List<int> target_ID_list, float rangeMultiplier = 1.0f)
        {
            // 安全檢查：發起者必須存在
            if (current_creature == null || current_creature.isDead) return 0;

            // 列表檢查：若清單為 null 或長度為 0，直接回傳 false
            if (target_ID_list == null || target_ID_list.Count == 0) return 0;

            return target_ID_list.Sum(id => CountTargetNumber(current_creature, id, rangeMultiplier));
        }
        // Retrieves a sorted list of all target creatures with the specified ID within perception range
        public static List<Creature> GetAllTargets(Creature current_creature, int target_ID, float rangeMultiplier = 1.0f, bool sorted = true)
        {
            // 安全檢查：發起偵測的生物必須存在且存活
            if (current_creature == null || current_creature.isDead)
                return new List<Creature>();

            // 檢查目標物種是否存在於 Manager 中
            if (!MainManager.inGameManager.Species.TryGetValue(target_ID, out var target_species))
                return new List<Creature>();
                
            Vector2 currentPos = current_creature.transform.position;
            float finalMultiplier = Mathf.Max(0f, rangeMultiplier);
            float range = current_creature.perceptionRange * finalMultiplier;
            float rangeSq = range * range;

            // 先進行基礎篩選
            var query = target_species.creatures.Values
                .Where(c => c != null && !c.isDead && c != current_creature &&
                            ((Vector2)c.transform.position - currentPos).sqrMagnitude < rangeSq);

            // 根據參數決定是否排序 
            if (sorted)
            {
                return query.OrderBy(c => ((Vector2)c.transform.position - currentPos).sqrMagnitude).ToList();
            }
            else
            {
                return query.ToList();
            }
        }
        // Retrieves a sorted list of all target creatures from the list of IDs within perception range
        public static List<Creature> GetAllTargets(Creature current_creature, List<int> target_ID_list, float rangeMultiplier = 1.0f, bool sorted = true)
        {
            // 安全檢查：發起者必須存在
            if (current_creature == null || current_creature.isDead) return new List<Creature>();

            // 列表檢查：若清單為 null 或長度為 0，直接回傳 false
            if (target_ID_list == null || target_ID_list.Count == 0) return new List<Creature>();
            
            Vector2 currentPos = current_creature.transform.position;

            var query = target_ID_list
                .SelectMany(id => GetAllTargets(current_creature, id, rangeMultiplier, false));
            if (sorted)
            {
                return query
                .OrderBy(c => ((Vector2)c.transform.position - currentPos).sqrMagnitude)
                .ToList();
            }
            else
            {
                return query.ToList();
            }
        }
    }

    public static class Items
    {
        
        private static List<Vector2Int> GetAllIntPos(Vector2Int pos, float radius)
        {
            float r2 = radius * radius;
            int radiusInt = Mathf.FloorToInt(radius);
            List<Vector2Int> positions = new();

            for (int dx = -radiusInt; dx <= radiusInt; dx++)
            {
                float remaining = r2 - dx * dx;

                if (remaining < 0) continue;
                int dy_limit = Mathf.FloorToInt(Mathf.Sqrt(remaining));
                int x = pos.x + dx;
                for (int dy = -dy_limit; dy <= dy_limit; dy++)
                {
                    int y = pos.y + dy;
                    positions.Add(new Vector2Int(x, y));
                }
            }

            return positions;
        }

        // Checks if there is at least one food item of the specified type within perception range
        public static bool HasTarget(Creature creature, FoodType food_type)
        {
            EntityData.SpawnableEntityType? spawnabletype = (EntityData.SpawnableEntityType)EntityData.FoodType2SpawnableType(food_type);
            if (spawnabletype == null)
            {
                Debug.LogError("Invalid food type: " + food_type.ToString());
                return false;
            }

            foreach (var ediblePos in GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.perceptionRange))
            {


                Edible edible = MainManager.inGameManager.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);
                if (edible == null || !edible.gameObject.activeInHierarchy) continue;
                if (edible.Type != food_type) continue;
                return true;
            }
            return false;
        }

        // Checks if there is at least one food item from the list of types within perception range
        public static bool HasTarget(Creature creature, List<FoodType> food_type_list)
        {
            var allPos = GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.perceptionRange);

            foreach (var food_type in food_type_list)
            {
                EntityData.SpawnableEntityType? spawnabletype = (EntityData.SpawnableEntityType)EntityData.FoodType2SpawnableType(food_type);
                if (spawnabletype == null)
                {
                    Debug.LogError("Invalid food type: " + food_type.ToString());
                    continue;
                }

                foreach (var ediblePos in allPos)
                {
                    Edible edible = MainManager.inGameManager.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);
                    if (edible == null || !edible.gameObject.activeInHierarchy) continue;
                    if (edible.Type != food_type) continue;
                    return true;
                }
            }
            return false;
        }

        // Counts the number of food items of the specified type within perception range
        public static int CountTargetNumber(Creature creature, FoodType food_type)
        {
            int count = 0;

            EntityData.SpawnableEntityType? spawnabletype = (EntityData.SpawnableEntityType)EntityData.FoodType2SpawnableType(food_type);
            if (spawnabletype == null)
            {
                Debug.LogError("Invalid food type: " + food_type.ToString());
                return 0;
            }

            foreach (var ediblePos in GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.perceptionRange))
            {
                Edible edible = MainManager.inGameManager.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);

                if (edible == null || !edible.gameObject.activeInHierarchy) continue;
                if (edible.Type != food_type) continue;
                count++;
            }
            return count;
        }

        // Counts the total number of food items from the list of types within perception range
        public static int CountTarget(Creature creature, List<FoodType> food_type_list)
        {
            int count = 0;
            var allPos = GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.perceptionRange);

            foreach (var food_type in food_type_list)
            {
                EntityData.SpawnableEntityType? spawnabletype = (EntityData.SpawnableEntityType)EntityData.FoodType2SpawnableType(food_type);
                if (spawnabletype == null)
                {
                    Debug.LogError("Invalid food type: " + food_type.ToString());
                    continue;
                }

                foreach (var ediblePos in allPos)
                {
                    Edible edible = MainManager.inGameManager?.EnvEntityManager?.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);

                    if (edible == null || !edible.gameObject.activeInHierarchy) continue;
                    if (edible.Type != food_type) continue;
                    count++;
                }
            }
            return count;
        }

        // Retrieves a list of all food items of the specified type within perception range
        public static List<Edible> GetAllTargets(Creature creature, FoodType food_type)
        {
            List<Edible> targets = new();
            var allPos = GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.perceptionRange);

            EntityData.SpawnableEntityType? spawnabletype = (EntityData.SpawnableEntityType)EntityData.FoodType2SpawnableType(food_type);
            if (spawnabletype == null)
            {
                Debug.LogError("Invalid food type: " + food_type.ToString());
                return targets;
            }

            foreach (var ediblePos in GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.perceptionRange))
            {
                Edible edible = MainManager.inGameManager.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);

                if (edible == null || !edible.gameObject.activeInHierarchy) continue;
                if (edible.Type != food_type) continue;
                targets.Add(edible);
            }

            targets.Sort((x, y) =>
            {
                float distanceX = Vector2.Distance(creature.transform.position, x.transform.position);
                float distanceY = Vector2.Distance(creature.transform.position, y.transform.position);
                return distanceX.CompareTo(distanceY);
            });
            return targets;
        }

        // Retrieves a sorted list of all food items from the list of types within perception range
        public static List<Edible> GetAllTargets(Creature creature, List<FoodType> food_type_list)
        {
            List<Edible> targets = new();
            var allPos = GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.perceptionRange);

            foreach (var food_type in food_type_list)
            {
                EntityData.SpawnableEntityType? spawnabletype = (EntityData.SpawnableEntityType)EntityData.FoodType2SpawnableType(food_type);
                if (spawnabletype == null)
                {
                    Debug.LogError("Invalid food type: " + food_type.ToString());
                    continue;
                }

                foreach (var ediblePos in allPos)
                {
                    Edible edible = MainManager.inGameManager.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);

                    if (edible == null || !edible.gameObject.activeInHierarchy) continue;
                    if (edible.Type != food_type) continue;
                    targets.Add(edible);
                }

            }
            targets.Sort((x, y) =>
            {
                float distanceX = Vector2.Distance(creature.transform.position, x.transform.position);
                float distanceY = Vector2.Distance(creature.transform.position, y.transform.position);
                return distanceX.CompareTo(distanceY);
            });
            return targets;
        }
    }
}
