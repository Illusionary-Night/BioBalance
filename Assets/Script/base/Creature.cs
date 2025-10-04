using System.Collections;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine;


public abstract class Creature : MonoBehaviour
{
    // ���a�M�w
    public float Size { get; set; }
    public float Speed { get; set; }
    public float BaseHealth { get; set; }
    public float ReproductionRate { get; set; }
    public float AttackPower { get; set; }
    public float Lifespan { get; }
    public float Variation { get; set; }
    public List<Creature> FoodList { get; set;  }       //�s�W�����C��
    public List<Creature> PredatorList { get; set;  }   //�s�W�ѼĦC��
    public List<Action> ActionList { get; set; }
    public int[] SleepingCycle { get; set; }

    // �q���p��
    public float HungerRate { get; }
    public float MaxHunger { get; }
    public float ReproductionInterval { get; }
    public float HealthRegeneration { get; }
    public DietType Diet { get; set; }
    public BodyType Body { get; set; }
    public int SleepTime { get; }

    // ��e���A
    public float Hunger { get; set; }
    public float Health { get; set; }
    public float Age { get; set; }
    public float ReproductionCooldown { get; set; }
    public int ActionCooldown { get; set; }

    public Creature(CreatureAttributes creatureAttributes)
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
        //��L���a�ݩʤ���
        Variation = creatureAttributes.variation;
        FoodList = creatureAttributes.food_list;
        PredatorList = creatureAttributes.predator_list;
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
    public void UpdateState()
    {
        //�^��B�j���B�Ѧ��B�c�ާN�o
        //�^��
        if(Health < BaseHealth)
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
    public void DoAction()
    {
        List<KeyValuePair<Action,float>> available_actions = new List<KeyValuePair<Action,float>>();
        //�C�^�X�}�l(�C�ͪ��y�{)	
        //�p��C��action������F���P�_
        //�p��C�ӹF������action���v��
        for (int i = 0; i < ActionList.Count; i++)
        {
            if (ActionList[i].isConditionMet())
            {
                available_actions.Add(new KeyValuePair<Action,float>(ActionList[i], ActionList[i].getWeight()));
            }
        }
        //�N����F����action�i���v���Ƨ�
        available_actions.Sort((x, y) => y.Value.CompareTo(x.Value));
        for (int i = 0; i < available_actions.Count; i++)
        {
            //����v���̰�
            Action selectedAction = available_actions[0].Key;
            //�릨�\�v
            if (selectedAction.isSuccess())
            {
                selectedAction.execute(this);
                return;
            }
            else
            {
                //����    ���v������
                available_actions.RemoveAt(0);
            }

        }
    }
}
