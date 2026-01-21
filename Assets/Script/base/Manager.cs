using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // Singleton instance
    public static Manager Instance { get; private set; }
    // List of all species in the simulation
    [SerializeField]
    private readonly List<Species> species = new();
    public List<Species> Species => species;
    // Initialize the EnvEntityManager
    public EnvEntityManager EnvEntityManager { get; private set; }

    // 便利屬性：提供對 EnvironmentEntities 的訪問
    public Transform EnvironmentEntities => EnvEntityManager?.EnvironmentEntities;

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
        
        // 在 Awake 中初始化 EnvEntityManager，確保在其他腳本的 Start 之前可用
        EnvEntityManager = new EnvEntityManager();
    }
    private void Initialize()
    {
        // Ensure TickManager exists
        if (TickManager.Instance == null)
        {
            new GameObject("TickManager").AddComponent<TickManager>();
        }
        
        // 啟用 EnvEntityManager 的 Tick 訂閱
        EnvEntityManager?.OnEnable();
    }

    private void OnDisable()
    {
        EnvEntityManager?.OnDisable();
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
}
