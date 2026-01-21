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
            float rangeSq = range * range; // �w����n�d�򪺥���

            return target_species.creatures.Values.Any(c =>
                c != null && !c.IsDead && c != current_creature &&
                (current_creature.transform.position - c.transform.position).sqrMagnitude < rangeSq
            );
        }
        // Returns true if any target creature from the list is found within perception range
        public static bool HasTarget(Creature creature, List<int> target_ID_list)
        {
            return target_ID_list?.Any(id => HasTarget(creature, id)) ?? false;
        }
        // Counts the number of target creatures with the specified ID within perception range
        public static int CountTargetNumber(Creature current_creature, int target_ID)
        {
            if (!Manager.Instance.Species.TryGetValue(target_ID, out var target_species))
                return 0;

            float range = current_creature.perceptionRange;
            float rangeSq = range * range; // �w����n�d�򪺥���

            return target_species.creatures.Values.Count(c =>
                c != null && !c.IsDead && c != current_creature &&
                (current_creature.transform.position - c.transform.position).sqrMagnitude < rangeSq
            );
        }
        // Counts the total number of target creatures from the list of IDs within perception range
        public static int CountTarget(Creature current_creature, List<int> target_ID_list)
        {
            return target_ID_list?.Sum(id => CountTargetNumber(current_creature, id)) ?? 0;
        }
        // Retrieves a sorted list of all target creatures with the specified ID within perception range
        public static List<Creature> GetAllTargets(Creature current_creature, int target_ID)
        {
            if (!Manager.Instance.Species.TryGetValue(target_ID, out var target_species))
                return new List<Creature>();

            Vector2 currentPos = current_creature.transform.position;
            float range = current_creature.perceptionRange;
            float rangeSq = range * range; // �w�⥭��H�u�Ʈį�

            return target_species.creatures.Values
                .Where(c => c != null && !c.IsDead && c != current_creature) // �L�o�L�ĥؼ�
                .Where(c => (currentPos - (Vector2)c.transform.position).sqrMagnitude < rangeSq) // �d��P�w
                .OrderBy(c => (currentPos - (Vector2)c.transform.position).sqrMagnitude) // �Ѫ�컷�Ƨ�
                .ToList(); // ��^ List
        }
        // Retrieves a sorted list of all target creatures from the list of IDs within perception range
        public static List<Creature> GetAllTargets(Creature current_creature, List<int> target_ID_list)
        {
            Vector2 currentPos = current_creature.transform.position;

            return target_ID_list?
                .SelectMany(id => GetAllTargets(current_creature, id)) // �N�h�� List �X�֬��@�ӧǦC
                .OrderBy(c => ((Vector2)c.transform.position - currentPos).sqrMagnitude) // �Τ@�i��Z���Ƨ�
                .ToList() ?? new List<Creature>(); // �T�O���^�� null
        }
    }

    public static class Items
    {
        // ���U��k�G���o�Ҧ����w��������������
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

            foreach (var ediblePos in GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.perceptionRange))
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
            var allPos = GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.perceptionRange);

            EntityData.SpawnableEntityType? spawnabletype = (EntityData.SpawnableEntityType)EntityData.FoodType2SpawnableType(food_type);
            if (spawnabletype == null)
            {
                Debug.LogError("Invalid food type: " + food_type.ToString());
                return targets;
            }

            foreach (var ediblePos in GetAllIntPos(Vector2Int.FloorToInt(creature.transform.position), creature.perceptionRange))
            {
                Edible edible = Manager.Instance.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);

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
                    Edible edible = Manager.Instance.EnvEntityManager.GetEntity<Edible>((EntityData.SpawnableEntityType)spawnabletype, ediblePos);

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
