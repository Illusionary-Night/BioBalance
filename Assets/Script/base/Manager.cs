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





        for (int i = 0; i < species.Count; i++)       //�ˬd�{���ͪ��C��
        {
            for (int j = 0; j < creatures[i].FoodList.Count; j++)       //�ˬd�{���ͪ��������C��
            {
                if (creatures[i].FoodList[j].GetType() == (new_creature.GetType()))     // new_creature �O creatures[i] ������
                {
                    // creatures[i] �|�y�� new_creature
                    if (!new_creature.PredatorList.Contains(creatures[i]))      //�קK���ƥ[�J
                    {
                        new_creature.PredatorList.Add(creatures[i]);        //�[�J�ѼĦC��
                    }
                }
            }
        }
        for (int i = 0; i < new_creature.FoodList.Count; i++)    //�ˬd new_creature �������C��
        {
            for (int j = 0; j < creatures.Count; j++)            //�ˬd�{���ͪ��C��
            {
                if (creatures[j].GetType() == (new_creature.FoodList[i].GetType()))     // creatures[j] �O new_creature ������
                {
                    // new_creature �|�y�� creatures[j]
                    if (!creatures[j].PredatorList.Contains(new_creature))      //�קK���ƥ[�J
                    {
                        creatures[j].PredatorList.Add(new_creature);        //�[�J�ѼĦC��
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
