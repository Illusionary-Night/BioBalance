using System.Collections;
using System.Collections.Generic;
using System;
//using Unity.VisualScripting;
using UnityEngine;


public abstract class Creature : MonoBehaviour, Tickable
{
    // ���a�M�w
    public GameObject GameObject {  get; private set; }
    public int SpeciesID { get; set; }
    public float Size { get; set; }
    public float Speed { get; set; }
    public float BaseHealth { get; set; }
    public float ReproductionRate { get; set; }
    public float AttackPower { get; set; }
    public float Lifespan { get; set;  }
    public float Variation { get; set; }
    public List<int> PreyIDList { get; set;  }       //�s�W�����C��
    public List<int> PredatorIDList { get; set;  }   //�s�W�ѼĦC��
    public List<ActionType> ActionList { get; set; }
    public int[] SleepingCycle { get; set; }
    public float PerceptionRange { get; set; }  // �P���d��

    // �q���p��
    public float HungerRate { get; set;  }
    public float MaxHunger { get; set;  }
    public float ReproductionInterval { get; set; }
    public float HealthRegeneration { get; set; }
    public DietType Diet { get; set; }
    public BodyType Body { get; set; }
    public int SleepTime { get; set; }

    // ��e���A
    public float Hunger { get; set; }
    public float Health { get; set; }
    public float Age { get; set; }
    public float ReproductionCooldown { get; set; }
    public int ActionCooldown { get; set; }

    public void Initialize(CreatureAttributes creatureAttributes)
    {
        float variationFactor() => UnityEngine.Random.Range(-creatureAttributes.variation, creatureAttributes.variation);
        //�ίv�ɶ��ܲ�
        int delta_sleep_time() => (int)(SleepTime * variationFactor());
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
        PreyIDList = creatureAttributes.prey_ID_list;
        PredatorIDList = creatureAttributes.predator_ID_list;
        ActionList = creatureAttributes.action_list;
        //�p��l���ݩ�
        HungerRate = AttributesCalculator.CalculateHungerRate(Size, Speed, AttackPower);
        MaxHunger = AttributesCalculator.CalculateMaxHunger(Size, BaseHealth, Diet);
        ReproductionInterval = AttributesCalculator.CalculateReproductionInterval(Size, BaseHealth);
        HealthRegeneration = AttributesCalculator.CalculateHealthRegeneration(BaseHealth, Size, SleepTime);
        SleepTime = SleepingCycle[1] - SleepingCycle[0];
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
        for (int i = 0; i < available_actions.Count; i++)
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
