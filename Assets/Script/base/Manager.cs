using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }
    [SerializeField]
    private readonly List<Species> species = new();
    public List<Species> Species => species;
    [SerializeField]
    private readonly Dictionary<Vector2Int, Edible> fooditems = new();
    public Dictionary<Vector2Int, Edible> FoodItems => fooditems;
    public static event System.Action OnTick;
    public Transform EnvironmentEntities { get; private set; }
    void Start()
    {
        Initialize();
    }
    private void Awake()
    {
        // One instance only
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Initialize()
    {
        if (TickManager.Instance == null)
        {
            new GameObject("TickManager").AddComponent<TickManager>();
        }

        TickManager.Instance.RegisterTickable(() =>
        {
            OnTick?.Invoke();
        });

        TickManager.Instance.RegisterTickable(SpawnEdible);

        GameObject env_entites_prefab = Resources.Load<GameObject>("Prefabs/Parents/EnvironmentEntities");
        EnvironmentEntities = Instantiate(env_entites_prefab).transform;
    }

    private void OnDestroy()
    {
        TickManager.Instance?.UnregisterTickable(SpawnEdible);
    }

    private void OnEnable()
    {
        OnTick += SpawnEdible;
    }
    private void OnDisable()
    {
        OnTick -= SpawnEdible;
    }


    private void PredatorUpdate(Creature new_creature)
    {
        foreach (var each_species in species)
        {
            if (each_species.creatures.Count == 0) continue;
            foreach (var each_prey_ID in each_species.creatures[0].PreyIDList)
            {
                if (each_prey_ID != new_creature.SpeciesID) continue;
                new_creature.PredatorIDList.Add(each_species.attributes.species_ID);
            }
        }
        foreach (var each_species in species)
        {
            foreach (var each_creature in each_species.creatures)
            {
                foreach (var each_prey_ID in new_creature.PreyIDList)
                {
                    if (each_prey_ID != each_creature.SpeciesID) continue;
                    bool is_duplicate = false;
                    foreach (var each_predator_ID in each_creature.PredatorIDList)
                    {
                        if (each_predator_ID == each_creature.SpeciesID) is_duplicate = true;
                    }
                    if (!is_duplicate) each_creature.PredatorIDList.Add(new_creature.SpeciesID);
                }
            }
        }
    }
    public void RegisterCreature(Creature new_creature)
    {
        //Debug.Log($"[Manager] 請求註冊生物, ID: {new_creature.SpeciesID}_{new_creature.UUID}");
        Species itsSpecies = new();
        bool is_new_species = true;
        foreach (var each_species in species)
        {
            if (each_species.attributes.species_ID == new_creature.SpeciesID)
            {
                is_new_species = false;
                itsSpecies = each_species;
            }
        }
        if (is_new_species)
        {
            itsSpecies = new();
            itsSpecies.creatures = new();
            itsSpecies.attributes = new_creature.ToCreatureAttribute();
            species.Add(itsSpecies);
        }
        itsSpecies.creatures.Add(new_creature);
        //PredatorUpdate(new_creature);
        // 列印目前的族群現況
        //foreach (var s in species)
        //{
        //    Debug.Log($"族群 {s.attributes.species_ID} 當前剩餘: {s.creatures.Count}");
        //    foreach(var c in s.creatures)
        //    {
        //        Debug.Log($"族群 {s.attributes.species_ID} 生物 {c.UUID}");
        //    }
        //}
    }
    public void UnregisterCreature(Creature dead_creature)
    {
        if (dead_creature == null) return;

        //Debug.Log($"[Manager] 請求註銷生物, ID: {dead_creature.SpeciesID}_{dead_creature.UUID}");
        bool success = false;

        // 使用 for 迴圈直接透過索引存取 List 內部的 struct
        for (int i = 0; i < species.Count; i++)
        {
            if (species[i].attributes.species_ID == dead_creature.SpeciesID)
            {
                for(int j = 0; j < species[i].creatures.Count; j++)
                {
                    if (species[i].creatures[j].UUID == dead_creature.UUID)
                    {
                        species[i].creatures.RemoveAt(j);
                    }
                }
                success = true; // 修正：記得標記成功
            }
        }

        if (!success)
        {
            Debug.LogError($"[Manager] 註銷失敗！找不到 SpeciesID 為 {dead_creature.SpeciesID} 的族群");
        }

        // 列印目前的族群現況
        //foreach (var s in species)
        //{
        //    Debug.Log($"族群 {s.attributes.species_ID} 當前剩餘: {s.creatures.Count}");
        //    foreach (var c in s.creatures)
        //    {
        //        Debug.Log($"族群 {s.attributes.species_ID} 生物 {c.UUID}");
        //    }
        //}
    }
    // spawn the edible item
    private void SpawnEdible()
    {
        //Debug.Log("grass num: "+FoodItems.Count);
        //TODO: Const of max food items and map size
        // Limit the number of food items
        if (fooditems.Count > 120) return;

        Vector2Int position = new Vector2Int(
            Random.Range(200, 300),
            Random.Range(200, 300)
        );
        // Check if position is occupied
        if (fooditems.ContainsKey(position)) return;
        // Check terrain type
        var random_positions = GetRandomPosition(position, 3, Random.Range(1, 5));
        foreach (var pos in random_positions)
        {
            //Debug.Log("Trying to spawn food at: " + pos);
            //Debug.Log("Terrain at pos: " + TerrainGenerator.Instance.GetDefinitionMap().GetTerrain(pos));
            if (fooditems.ContainsKey(pos)) continue;
            if (TerrainGenerator.Instance.GetDefinitionMap().GetTerrain(pos) != TerrainType.Grass) continue;
            Vector3 pos3 = new Vector3(pos.x, pos.y, 0f);
            // Spawn food item
            GameObject food_item_prefab = Resources.Load<GameObject>("Prefabs/Edible/Grass");
            GameObject food_item_object = Instantiate(food_item_prefab, pos3, Quaternion.identity,EnvironmentEntities);
            Edible food_item = food_item_object.GetComponent<Edible>();
            fooditems.Add(pos, food_item);
            food_item.Initialize();

        }

    }

    private List<Vector2Int> GetRandomPosition(Vector2Int position, int r, int n)
    {
        List<Vector2Int> position_ = new();
        HashSet<Vector2Int> used_position = new();
        uint tries = 0;
        while (position_.Count < n && tries < n * 10)
        {
            float angle = Random.Range(0f, 360f);
            float radius = Random.Range(0f, r);
            Vector2Int new_position = position + new Vector2Int(
                Mathf.RoundToInt(radius * Mathf.Cos(angle * Mathf.Deg2Rad)),
                Mathf.RoundToInt(radius * Mathf.Sin(angle * Mathf.Deg2Rad))
            );
            if (!used_position.Contains(new_position))
            {
                used_position.Add(new_position);
                position_.Add(new_position);
            }
            tries++;
        }
        return position_;
    }
}
