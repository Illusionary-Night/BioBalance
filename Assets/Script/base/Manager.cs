using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Manager : MonoBehaviour
{
    public static Manager Instance { get; private set; }
    [SerializeField] private List<Species> species;
    public List<Species> Species => species;
    public static event System.Action OnTick;
    public static List<Edible> FoodItems;
    public float tickInterval = 1f / 30; // ¨C­Ó¹CÀ¸³æ¦ì®É¶¡ (¬í)
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
        //  ?²æ­¢?´æ™¯?‡æ?å¾Œé?è¤‡å»ºç«?Manager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        species = new List<Species>();
    }
    // Update is called once per frame
    void Update()
    {
        tick_timer += Time.deltaTime;
        if (tick_timer >= tickInterval)
        {
            tick_timer -= tickInterval;
            TickTime = (TickTime + 1) % mixTickTime;
            // ¦b³o¸Ì³B²z¨C­Ó¹CÀ¸®É¶¡³æ¦ìªºÅŞ¿è
            OnTick?.Invoke();
        }
    }
    private void Initialize()
    {
        TickTime = 0;
    }
    private void PredatorUpdate(Creature new_creature)
    {
        foreach (var each_species in species)   //?°ç??©ç?å¤©æ•µ?å–®è£œå?
        {
            if (each_species.creatures.Count == 0)continue;
            foreach (var each_prey_ID in each_species.creatures[0].PreyIDList)
            {
                if (each_prey_ID != new_creature.SpeciesID) continue;
                new_creature.PredatorIDList.Add(each_species.attributes.species_ID);
            }
        }
        foreach (var each_species in species)   //?Šç??©ç?å¤©æ•µ?å–®è£œå?
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
        if (is_new_species) {
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
        foreach(var each_species in species)
        {
            if (each_species.attributes.species_ID == dead_creature.SpeciesID)
            {
                each_species.creatures.Remove(dead_creature);
            }
        }
    }
    
}
