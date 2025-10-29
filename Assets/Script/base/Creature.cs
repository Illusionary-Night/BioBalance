using System.Collections;
using System.Collections.Generic;
using System;
//using Unity.VisualScripting;
using UnityEngine;


public abstract class Creature : MonoBehaviour, Tickable
{
    // ���a�M�w
    [Header("=== ���a�M�w ===")]
    public GameObject CreatureObject {  get; private set; }
    [SerializeField] private int species_ID;
    public int SpeciesID { get => species_ID; set => species_ID = value; }
    [SerializeField] private float size;
    public float Size { get => size; set => size = value; }
    [SerializeField] private float speed;
    public float Speed { get => speed; set => speed = value; }
    [SerializeField] private float base_health; 
    public float BaseHealth { get => base_health; set => base_health = value; }
    [SerializeField] private float reproduction_rate;
    public float ReproductionRate { get => reproduction_rate; set => reproduction_rate = value; }
    [SerializeField] private float attack_power; 
    public float AttackPower { get => attack_power; set => attack_power = value; }
    [SerializeField] private float lifespan; 
    public float Lifespan { get => lifespan; set => lifespan = value; }
    [SerializeField] private float variation;
    public float Variation { get => variation; set => variation = value; }
    [SerializeField] private List<int> prey_ID_list = new List<int>();
    public List<int> PreyIDList { get => prey_ID_list; set => prey_ID_list = value; }       //�s�W�����C��
    [SerializeField] private List<int> predator_ID_list = new List<int>();
    public List<int> PredatorIDList { get => predator_ID_list; set => predator_ID_list = value; }   //�s�W�ѼĦC��
    [SerializeField] private List<ActionType> action_list;
    public List<ActionType> ActionList { get => action_list; set => action_list = value; }

    [SerializeField] private int[] sleepingCycle;
    public int[] SleepingCycle { get => sleepingCycle; set => sleepingCycle = value; }

    [SerializeField] private float perceptionRange;  // �P���d��
    public float PerceptionRange { get => perceptionRange; set => perceptionRange = value; }

    // �q���p��
    [Header("=== �q���p�� ===")]
    [SerializeField] private float hungerRate;
    public float HungerRate { get => hungerRate; set => hungerRate = value; }

    [SerializeField] private float maxHunger;
    public float MaxHunger { get => maxHunger; set => maxHunger = value; }

    [SerializeField] private float reproductionInterval;
    public float ReproductionInterval { get => reproductionInterval; set => reproductionInterval = value; }

    [SerializeField] private float healthRegeneration;
    public float HealthRegeneration { get => healthRegeneration; set => healthRegeneration = value; }

    [SerializeField] private DietType diet;
    public DietType Diet { get => diet; set => diet = value; }

    [SerializeField] private BodyType body;
    public BodyType Body { get => body; set => body = value; }

    [SerializeField] private int sleepTime;
    public int SleepTime { get => sleepTime; set => sleepTime = value; }

    // ��e���A
    [Header("=== ��e���A ===")]
    [SerializeField] private float hunger;
    public float Hunger { get => hunger; set => hunger = value; }

    [SerializeField] private float health;
    public float Health { get => health; set => health = value; }

    [SerializeField] private float age;
    public float Age { get => age; set => age = value; }

    [SerializeField] private float reproductionCooldown;
    public float ReproductionCooldown { get => reproductionCooldown; set => reproductionCooldown = value; }

    [SerializeField] private int actionCooldown;
    public int ActionCooldown { get => actionCooldown; set => actionCooldown = value; }

    public void Initialize(CreatureAttributes creatureAttributes)
    {
        float variationFactor() => UnityEngine.Random.Range(-creatureAttributes.variation, creatureAttributes.variation);
        //�ίv�ɶ��ܲ�
        int delta_sleep_time() => (int)((creatureAttributes.sleeping_cycle[1]-creatureAttributes.sleeping_cycle[0]) * variationFactor());
        SleepingCycle = new int[2];
        SleepingCycle[0] = creatureAttributes.sleeping_cycle[0] + delta_sleep_time();
        SleepingCycle[1] = creatureAttributes.sleeping_cycle[1] + delta_sleep_time();
        //��L���a�ݩ��ܲ�
        Size = creatureAttributes.size + creatureAttributes.size * variationFactor();
        Speed = creatureAttributes.speed + creatureAttributes.speed * variationFactor();
        BaseHealth = creatureAttributes.base_health + creatureAttributes.base_health * variationFactor();
        ReproductionRate = creatureAttributes.reproduction_rate + creatureAttributes.reproduction_rate * variationFactor();
        AttackPower = creatureAttributes.attack_power + creatureAttributes.attack_power * variationFactor();
        Lifespan = creatureAttributes.lifespan + creatureAttributes.lifespan * variationFactor();
        PerceptionRange = creatureAttributes.perception_range + creatureAttributes.perception_range * variationFactor();
        //��L���a�ݩʤ���
        SpeciesID = creatureAttributes.species_ID;
        Variation = creatureAttributes.variation;
        PreyIDList = new List<int>(creatureAttributes.prey_ID_list);
        PredatorIDList = new List<int>(creatureAttributes.predator_ID_list);
        ActionList = new List<ActionType>(creatureAttributes.action_list);
        //�p��l���ݩ�
        SleepTime = SleepingCycle[1] - SleepingCycle[0];
        HungerRate = AttributesCalculator.CalculateHungerRate(Size, Speed, AttackPower);
        MaxHunger = AttributesCalculator.CalculateMaxHunger(Size, BaseHealth, Diet);
        ReproductionInterval = AttributesCalculator.CalculateReproductionInterval(Size, BaseHealth);
        HealthRegeneration = AttributesCalculator.CalculateHealthRegeneration(BaseHealth, Size, SleepTime);
        //��l���A
        Hunger = MaxHunger;
        Health = BaseHealth;
        Age = 0;
        ReproductionCooldown = 0;
        ActionCooldown = 0;
    }
    public void DoAction()
    {
        List<KeyValuePair<ActionType,float>> available_actions = new();
        //�C�^�X�}�l(�C�ͪ��y�{)	
        //�p��C��action������F���P�_
        //�p��C�ӹF������action���v��
        for (int i = 0; i < ActionList.Count; i++)
        {
            if (ActionSystem.IsConditionMet(this, ActionList[i]))
            {
                available_actions.Add(new KeyValuePair<ActionType,float>(ActionList[i], ActionSystem.GetWeight(this,ActionList[i])));
            }
        }
        //�N����F����action�i���v���Ƨ�
        available_actions.Sort((x, y) => y.Value.CompareTo(x.Value));
        while (available_actions.Count > 0)
        {
            //����v���̰�
            ActionType selectedAction = available_actions[0].Key;
            //�릨�\�v
            if (ActionSystem.IsSuccess(this,selectedAction))
            {
                ActionSystem.Execute(this, selectedAction);

                return;
            }
            else
            {
                //����    ���v������
                available_actions.RemoveAt(0);
            }

        }
    }
    public CreatureAttributes ToCreatureAttribute()
    {
        CreatureAttributes attributes = new CreatureAttributes();
        attributes.species_ID = SpeciesID;
        attributes.size = Size;
        attributes.base_health = BaseHealth;
        attributes.speed = Speed;
        attributes.attack_power = AttackPower;
        attributes.reproduction_rate = ReproductionRate;
        attributes.variation = Variation;
        attributes.lifespan = Lifespan;
        attributes.perception_range = PerceptionRange;
        attributes.sleeping_cycle = SleepingCycle;
        attributes.Diet = Diet;
        attributes.Body = Body;
        attributes.prey_ID_list = PreyIDList;
        attributes.predator_ID_list = PredatorIDList;
        attributes.action_list = ActionList;
        return attributes;
    }
    public void OnTick()
    {
        //�^��B�j���B�Ѧ��B�c�ާN�o
        //�^��
        if (Health < BaseHealth)
        {
            Health += HealthRegeneration;
        }
        Health = Mathf.Min(Health, BaseHealth);
        //�j��
        Hunger -= HungerRate;
        if (Hunger <= 0)
        {
            //Debug.Log("�j��");
        }
        //�Ѧ�
        Age += 1;
        if (Age >= Lifespan)
        {
            //Debug.Log("�Ѧ�");
        }
        //�c�ާN�o
        if (ReproductionCooldown > 0)
        {
            ReproductionCooldown -= 1;
        }

        //��ʧN�o
        if (ActionCooldown > 0)
        {
            ActionCooldown -= 1;
        }

        if (ActionCooldown <= 0)
        {
            DoAction();
        }
    }
}
