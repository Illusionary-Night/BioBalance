using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

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

    public static class Items
    {
        // 輔助方法：取得所有指定類型的食物實體
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

            foreach (var ediblePos in GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.PerceptionRange))
            {
                
                
                Edible edible = Manager.Instance.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);
                if (edible == null || !edible.gameObject.activeInHierarchy) continue;
                if (edible.Type != food_type) continue;
                return true;
            }
            return false;
        }

        // Checks if there is at least one food item from the list of types within perception range
        public static bool HasTarget(Creature creature, List<FoodType> food_type_list)
        {
            var allPos = GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.PerceptionRange);

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
                    Edible edible = Manager.Instance.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);
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

            foreach (var ediblePos in GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.PerceptionRange))
            {
                Edible edible = Manager.Instance.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);

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
            var allPos = GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.PerceptionRange);

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
                    Edible edible = Manager.Instance.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);

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
            var allPos = GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.PerceptionRange);

            EntityData.SpawnableEntityType? spawnabletype = (EntityData.SpawnableEntityType)EntityData.FoodType2SpawnableType(food_type);
            if (spawnabletype == null)
            {
                Debug.LogError("Invalid food type: " + food_type.ToString());
                return targets;
            }

            foreach (var ediblePos in GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.PerceptionRange))
            {
                Edible edible = Manager.Instance.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);

                if (edible == null || !edible.gameObject.activeInHierarchy) continue;
                if (edible.Type != food_type) continue;
                targets.Add(edible);
            }

            targets.Sort((x, y) => {
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
            var allPos = GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.PerceptionRange);

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
                    Edible edible = Manager.Instance.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);

                    if (edible == null || !edible.gameObject.activeInHierarchy) continue;
                    if (edible.Type != food_type) continue;
                    targets.Add(edible);
                }

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
