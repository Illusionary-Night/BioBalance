using System.Collections;
using System.Collections.Generic;
using System;
//using Unity.VisualScripting;
using UnityEngine;


public abstract class Creature : MonoBehaviour, Tickable
{
    // 玩家決定
    public GameObject GameObject {  get; private set; }
    public int SpeciesID { get; set; }
    public float Size { get; set; }
    public float Speed { get; set; }
    public float BaseHealth { get; set; }
    public float ReproductionRate { get; set; }
    public float AttackPower { get; set; }
    public float Lifespan { get; set;  }
    public float Variation { get; set; }
    public List<int> PreyIDList { get; set;  }       //新增食物列表
    public List<int> PredatorIDList { get; set;  }   //新增天敵列表
    public List<ActionType> ActionList { get; set; }
    public int[] SleepingCycle { get; set; }
    public float PerceptionRange { get; set; }  // 感知範圍

    // 電腦計算
    public float HungerRate { get; set;  }
    public float MaxHunger { get; set;  }
    public float ReproductionInterval { get; set; }
    public float HealthRegeneration { get; set; }
    public DietType Diet { get; set; }
    public BodyType Body { get; set; }
    public int SleepTime { get; set; }

    // 當前狀態
    public float Hunger { get; set; }
    public float Health { get; set; }
    public float Age { get; set; }
    public float ReproductionCooldown { get; set; }
    public int ActionCooldown { get; set; }

    public void Initialize(CreatureAttributes creatureAttributes)
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
        PerceptionRange = creatureAttributes.perception_range + creatureAttributes.perception_range * variationFactor();
        //其他玩家屬性不變
        SpeciesID = creatureAttributes.species_ID;
        Variation = creatureAttributes.variation;
        PreyIDList = creatureAttributes.prey_ID_list;
        PredatorIDList = creatureAttributes.predator_ID_list;
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
    public void DoAction()
    {
        List<KeyValuePair<ActionType,float>> available_actions = new();
        //每回合開始(每生物流程)	
        //計算每個action的條件達成與否
        //計算每個達成條件的action的權重
        for (int i = 0; i < ActionList.Count; i++)
        {
            if (ActionSystem.IsConditionMet(this, ActionList[i]))
            {
                available_actions.Add(new KeyValuePair<ActionType,float>(ActionList[i], ActionSystem.GetWeight(this,ActionList[i])));
            }
        }
        //將條件達成的action進行權重排序
        available_actions.Sort((x, y) => y.Value.CompareTo(x.Value));
        for (int i = 0; i < available_actions.Count; i++)
        {
            //選擇權重最高
            ActionType selectedAction = available_actions[0].Key;
            //骰成功率
            if (ActionSystem.IsSuccess(this,selectedAction))
            {
                ActionSystem.Execute(this, selectedAction);

                return;
            }
            else
            {
                //失敗    找權重次高
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
        //回血、餓死、老死、繁殖冷卻
        //回血
        if (Health < BaseHealth)
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
}
