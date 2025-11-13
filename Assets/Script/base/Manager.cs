using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    public static event System.Action OnTick;
    public static List<Species> species;
    public static List<Edible> FoodItems;
    public float tickInterval = 1f / 30; // 每個遊戲單位時間 (秒)
    private float tickTimer = 0;
    private int mixTickTime = 240000;
    private int tickTime = 0;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        tickTimer += Time.deltaTime;
        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            tickTime = (tickTime + 1) % mixTickTime;
            // 在這裡處理每個遊戲時間單位的邏輯
            OnTick?.Invoke();
        }
    }
    private void Initialize()
    {
    }
    private void PredatorUpdate(Creature new_creature)
    {
        foreach (var each_species in species)   //新生物的天敵名單補充
        {
            if (each_species.creatures.Count == 0)continue;
            foreach (var each_prey_ID in each_species.creatures[0].PreyIDList)
            {
                if (each_prey_ID != new_creature.SpeciesID) continue;
                new_creature.PredatorIDList.Add(each_species.attributes.species_ID);
            }
        }
        foreach (var each_species in species)   //舊生物的天敵名單補充
        {
            foreach(var each_creature in each_species.creatures)
            {
                foreach(var each_prey_ID in new_creature.PreyIDList)
                {
                    if (each_prey_ID != each_creature.SpeciesID) continue;
                    bool is_duplicate = false;
                    foreach (var each_predator_ID in each_creature.PredatorIDList)
                    {
                        if(each_predator_ID == each_creature.SpeciesID)is_duplicate = true;
                    }
                    if(!is_duplicate)each_creature.PredatorIDList.Add(new_creature.SpeciesID);
                }
            }
        }
    }
    private void AddCreature(Creature new_creature)
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
        if (is_new_species) {
            Species new_species = new Species();
            new_species.creatures.Add(new_creature);
            new_species.attributes = new_creature.ToCreatureAttribute();
            species.Add(new_species);
        }
        PredatorUpdate(new_creature);
    }
    private void RemoveCreature(Creature dead_creature)
    {
        foreach(var each_species in species)
        {
            if (each_species.attributes.species_ID == dead_creature.SpeciesID)
            {
                each_species.creatures.Remove(dead_creature);
            }
        }
    }
    
}
