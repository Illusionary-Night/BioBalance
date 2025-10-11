using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Manager : MonoBehaviour
{
    public static List<Species> species;
    private List<GameObject> meat;
    private List<GameObject> grass;
    private List<Tickable> tickables;
    public float tickInterval = 1f; // 每個遊戲單位時間 = 1 秒
    private float tickTimer = 0;
    private int mixTickTime = 240000;
    private int tickTime = 0;
    public Dictionary<int, Species> species_dictionary = new Dictionary<int, Species>();
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
            foreach (var tickable in tickables)
            {
                tickable.OnTick();
            }
        }
    }
    private void Initialize()
    {
    }
    private void PredatorUpdate(Creature new_creature)
    {
        foreach (var each_species in species)
        {
            if (each_species.creatures.Count == 0)continue;
            foreach (var each_prey_ID in each_species.creatures[0].PreyIDList)
            {
                if (each_prey_ID != new_creature.SpeciesID) continue;
                bool is_duplicate = false;
                foreach (var each_predator_ID in new_creature.PredatorIDList)
                {
                    if (each_predator_ID == each_species.attributes.species_ID) is_duplicate = true;
                }
                if (!is_duplicate) new_creature.PredatorIDList.Add(each_species.attributes.species_ID);
            }
        }
        foreach (var each_species in species)
        {
            foreach (var each_prey_ID in new_creature.PreyIDList)
            {
                if (each_prey_ID != each_species.attributes.species_ID) continue;
                bool is_duplicate = false;
                foreach (var each_predator_ID in new_creature.PredatorIDList)
                {
                    if (each_predator_ID == each_creature.SpeciesID) is_duplicate = true;
                }
                if (!is_duplicate) new_creature.PredatorIDList.Add(each_creature.SpeciesID);
            }
            foreach (var each_creature in each_species.creatures)
        }





        for (int i = 0; i < species.Count; i++)       //檢查現有生物列表
        {
            for (int j = 0; j < creatures[i].FoodList.Count; j++)       //檢查現有生物的食物列表
            {
                if (creatures[i].FoodList[j].GetType() == (new_creature.GetType()))     // new_creature 是 creatures[i] 的食物
                {
                    // creatures[i] 會獵捕 new_creature
                    if (!new_creature.PredatorList.Contains(creatures[i]))      //避免重複加入
                    {
                        new_creature.PredatorList.Add(creatures[i]);        //加入天敵列表
                    }
                }
            }
        }
        for (int i = 0; i < new_creature.FoodList.Count; i++)    //檢查 new_creature 的食物列表
        {
            for (int j = 0; j < creatures.Count; j++)            //檢查現有生物列表
            {
                if (creatures[j].GetType() == (new_creature.FoodList[i].GetType()))     // creatures[j] 是 new_creature 的食物
                {
                    // new_creature 會獵捕 creatures[j]
                    if (!creatures[j].PredatorList.Contains(new_creature))      //避免重複加入
                    {
                        creatures[j].PredatorList.Add(new_creature);        //加入天敵列表
                    }
                }
            }
        }
    }
    private void AddCreature(Creature new_creature)
    {
        creatures.Add(new_creature);
        PredatorUpdate(new_creature);
    }
    private void RemoveCreature(Creature dead_creature)
    {
        creatures.Remove(dead_creature);
    }
    
}
