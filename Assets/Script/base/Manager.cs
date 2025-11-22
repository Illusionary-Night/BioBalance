using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }
    [SerializeField] private readonly List<Species> species = new();
    [SerializeField] private readonly Dictionary<Vector2Int, Edible> fooditems = new();
    public List<Species> Species => species;
    public Dictionary<Vector2Int, Edible> FoodItems => fooditems;
    public static event System.Action OnTick;
    public float tickInterval = 1f / 30; // 30 ticks per second
    private float tick_timer = 0;
    private int mixTickTime = 240000;
    [SerializeField] public int TickTime;
    private int tick_time => tick_time;
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
    // Update is called once per frame
    void Update()
    {
        tick_timer += Time.deltaTime;
        if (tick_timer >= tickInterval)
        {
            tick_timer -= tickInterval;
            TickTime = (TickTime + 1) % mixTickTime;
            // Trigger event
            OnTick?.Invoke();
        }
    }
    private void Initialize()
    {
        TickTime = 0;
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
        bool is_new_species = true;
        foreach (var each_species in species)
        {
            if (each_species.attributes.species_ID == new_creature.SpeciesID)
            {
                is_new_species = false;
                each_species.creatures.Add(new_creature);
            }
        }
        if (is_new_species)
        {
            Species new_species = new();
            new_species.creatures = new();
            new_species.creatures.Add(new_creature);
            new_species.attributes = new_creature.ToCreatureAttribute();
            species.Add(new_species);
        }
        //PredatorUpdate(new_creature);
    }
    public void UnregisterCreature(Creature dead_creature)
    {
        foreach (var each_species in species)
        {
            if (each_species.attributes.species_ID == dead_creature.SpeciesID)
            {
                each_species.creatures.Remove(dead_creature);
            }
        }
    }
    // spawn the edible item
    private void SpawnEdible()
    {
        //TODO: Const of max food items and map size
        // Limit the number of food items
        if (fooditems.Count > 120) return;

        Vector2Int position = new Vector2Int(
            Random.Range(-50, 50),
            Random.Range(-50, 50)
        );
        // Check if position is occupied
        if (fooditems.ContainsKey(position)) return;
        // Check terrain type
        var random_positions = GetRandomPosition(position, 3, Random.Range(1, 5));
        foreach (var pos in random_positions)
        {
            if (fooditems.ContainsKey(pos)) continue;
            if (TerrainGenerator.Instance.GetDefinitionMap().GetTerrain(pos) != TerrainType.Grass) continue;
            // Spawn food item
            GameObject food_item_prefab = Resources.Load<GameObject>("Prefabs/Edible/Grass");
            GameObject food_item_object = Instantiate(food_item_prefab);
            Edible food_item = food_item_object.GetComponent<Edible>();
            food_item.Initialize(pos);
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
