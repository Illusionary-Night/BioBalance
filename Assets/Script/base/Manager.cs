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
    public float tickInterval = 1f; // �C�ӹC�����ɶ� = 1 ��
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
            // �b�o�̳B�z�C�ӹC���ɶ���쪺�޿�
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
        foreach (var each_species in species)   //�s�ͪ����ѼĦW��ɥR
        {
            if (each_species.creatures.Count == 0)continue;
            foreach (var each_prey_ID in each_species.creatures[0].PreyIDList)
            {
                if (each_prey_ID != new_creature.SpeciesID) continue;
                new_creature.PredatorIDList.Add(each_species.attributes.species_ID);
            }
        }
        foreach (var each_species in species)   //�¥ͪ����ѼĦW��ɥR
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
