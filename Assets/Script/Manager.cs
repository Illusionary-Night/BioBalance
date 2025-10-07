using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    private List<Creature> creatures;
    private List<GameObject> meat;
    private List<GameObject> grass;
    private List<CreatureAttributes> species;
    public float tickInterval = 1f; // �C�ӹC�����ɶ� = 1 ��
    private float tickTimer = 0f;

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
            OnTick(); // Ĳ�o�@�ӹC���ɶ����
        }
    }
    private void Initialize()
    {
        CreatureAttributes new_species = new CreatureAttributes();
        new_species.size = 1f;
        new_species.speed = 1f;
        new_species.base_health = 10f;
        new_species.reproduction_rate = 0.1f;
        new_species.attack_power = 2f;
        new_species.lifespan = 100f;
        new_species.variation = 0.1f;
        new_species.sleeping_cycle = new int[] { 20, 6 };
        new_species.Diet = DietType.Carnivore;
        new_species.Body = BodyType.Medium;
        new_species.food_list = new List<Creature>();
        new_species.predator_list = new List<Creature>();
        new_species.action_list = new List<Action>();
        species = new List<CreatureAttributes>();
        species.Add(new_species);
        creatures = new List<Creature>();
        meat = new List<GameObject>();
        grass = new List<GameObject>();
        // ��l�ƥͪ��C��
        for (int i = 0; i < 10; i++)   //�ͦ�10����l�ͪ�
        {
            GameObject creatureObject = new GameObject("Creature_" + i);
            Creature creatureComponent = creatureObject.AddComponent<Creature>();
            creatureComponent.Initialize(new_species);
            AddCreature(creatureComponent);
        }
    }
    private void OnTick()
    {
        // �b�o�̳B�z�C�ӹC���ɶ���쪺�޿�
        foreach (var creature in creatures)
        {
            // ��s�ͪ������A�P�欰
            creature.UpdateState();
        }
    }
    private void PredatorUpdate(Creature new_creature)
    {
        for (int i = 0; i < creatures.Count; i++)       //�ˬd�{���ͪ��C��
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
