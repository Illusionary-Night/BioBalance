using System.Collections;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine;


public abstract class Creature : MonoBehaviour
{
    // 玩家決定
    public float Size { get; set; }
    public float Speed { get; set; }
    public float BaseHealth { get; set; }
    public float ReproductionRate { get; set; }
    public float AttackPower { get; set; }
    public float Lifespan { get; }
    public float Variation { get; set; }
    public List<Creature> FoodList { get; set;  }       //新增食物列表
    public List<Creature> PredatorList { get; set;  }   //新增天敵列表
    public List<Action> ActionList { get; set; }
    public int[] SleepingCycle { get; set; }

    // 電腦計算
    public float HungerRate { get; }
    public float MaxHunger { get; }
    public float ReproductionInterval { get; }
    public float HealthRegeneration { get; }
    public DietType Diet { get; set; }
    public BodyType Body { get; set; }
    public int SleepTime { get; }

    // 當前狀態
    public float Hunger { get; set; }
    public float Health { get; set; }
    public float Age { get; set; }
    public float ReproductionCooldown { get; set; }
    public int ActionCooldown { get; set; }

    public Creature(CreatureAttributes creatureAttributes)
    {
        float variationFactor() => UnityEngine.Random.Range(-creatureAttributes.variation, creatureAttributes.variation);
        //睡眠時間變異
        int delta_sleep_time() => (int)(SleepTime * variationFactor());
        SleepingCycle = new int[2];
        SleepingCycle[0] = creatureAttributes.sleeping_cycle[0] + delta_sleep_time();
        SleepingCycle[1] = creatureAttributes.sleeping_cycle[1] + delta_sleep_time();
        //其他玩家屬性變異
        Size = creatureAttributes.size + creatureAttributes.size * variationFactor();
        Speed = creatureAttributes.speed + creatureAttributes.speed * variationFactor();
        BaseHealth = creatureAttributes.base_health + creatureAttributes.base_health * variationFactor();
        ReproductionRate = creatureAttributes.reproduction_rate + creatureAttributes.reproduction_rate * variationFactor();
        AttackPower = creatureAttributes.attack_power + creatureAttributes.attack_power * variationFactor();
        Lifespan = creatureAttributes.lifespan + creatureAttributes.lifespan * variationFactor();
        //其他玩家屬性不變
        Variation = creatureAttributes.variation;
        FoodList = creatureAttributes.food_list;
        PredatorList = creatureAttributes.predator_list;
        ActionList = creatureAttributes.action_list;
        //計算衍生屬性
        HungerRate = AttributesCalculator.CalculateHungerRate(Size, Speed, AttackPower);
        MaxHunger = AttributesCalculator.CalculateMaxHunger(Size, BaseHealth, Diet);
        ReproductionInterval = AttributesCalculator.CalculateReproductionInterval(Size, BaseHealth);
        HealthRegeneration = AttributesCalculator.CalculateHealthRegeneration(BaseHealth, Size, SleepTime);
        SleepTime = SleepingCycle[1] - SleepingCycle[0];
        //初始狀態
        Hunger = MaxHunger;
        Health = BaseHealth;
        Age = 0;
        ReproductionCooldown = 0;
        ActionCooldown = 0;
    }
    public void UpdateState()
    {
        //回血、餓死、老死、繁殖冷卻
        //回血
        if(Health < BaseHealth)
        {
            Health += HealthRegeneration;
        }
        //餓死
        Hunger -= HungerRate;
        if (Hunger <= 0)
        {
            //Debug.Log("餓死");
        }
        //老死
        Age += 1;
        if (Age >= Lifespan)
        {
            //Debug.Log("老死");
        }
        //繁殖冷卻
        if (ReproductionCooldown > 0)
        {
            ReproductionCooldown -= 1;
        }

        //行動冷卻
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
        //每回合開始(每生物流程)	
        //計算每個action的條件達成與否
        //計算每個達成條件的action的權重
        for (int i = 0; i < ActionList.Count; i++)
        {
            if (ActionList[i].isConditionMet())
            {
                available_actions.Add(new KeyValuePair<Action,float>(ActionList[i], ActionList[i].getWeight()));
            }
        }
        //將條件達成的action進行權重排序
        available_actions.Sort((x, y) => y.Value.CompareTo(x.Value));
        for (int i = 0; i < available_actions.Count; i++)
        {
            //選擇權重最高
            Action selectedAction = available_actions[0].Key;
            //骰成功率
            if (selectedAction.isSuccess())
            {
                selectedAction.execute(this);
                return;
            }
            else
            {
                //失敗    找權重次高
                available_actions.RemoveAt(0);
            }

        }
    }
}
