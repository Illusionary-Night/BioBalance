using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum BodyType
{
    small,
    medium,
    large
}
public enum DietType
{
    Herbivore,  //草食
    Carnivore,  //肉食
    Omnivore    //雜食
}
public abstract class creature
{
    // 玩家決定
    public float Size { get; set; }
    public float Speed { get; set; }
    public float BaseHealth { get; set; }
    public float ReproductionRate { get; set; }
    public float AttackPower { get; set; }
    public List<creature> FoodList { get; set; }
    public string TypeTag { get; set; }
    public string SleepCycle { get; set; }
    public DietType Diet { get; set; }
    public BodyType Body { get; set; }
    public List<action> ActionList { get; set; }

    // 電腦計算
    public float HungerRate { get; set; }
    public float MaxHunger { get; set; }
    public float Lifespan { get; set; }
    public float ReproductionInterval { get; set; }

    // 當前狀態
    public float Hunger { get; set; }
    public float Health { get; set; }
    public float Age { get; set; }
    public float ReproductionCooldown { get; set; }
    // Start is called before the first frame update
    public void action()
    {
        List<KeyValuePair<action,float>> availableActions = new List<KeyValuePair<action,float>>();
        //每回合開始(每生物流程)	
        //計算每個action的條件達成與否
        //計算每個達成條件的action的權重
        for (int i = 0; i < ActionList.Count; i++)
        {
            if (ActionList[i].isConditionMet())
            {
                availableActions.Add(new KeyValuePair<action,float>(ActionList[i], ActionList[i].getWeight()));
            }
        }
        //將條件達成的action進行權重排序
        availableActions.Sort((x, y) => y.Value.CompareTo(x.Value));
        for (int i = 0; i < availableActions.Count; i++)
        {
            //選擇權重最高
            action selectedAction = availableActions[0].Key;
            //骰成功率
            if (selectedAction.isSuccess())
            {
                selectedAction.execute();
                break;
            }
            else
            {
                //失敗    找權重次高
                availableActions.RemoveAt(0);
            }

        }
    }
}
public class action
{
    public bool isConditionMet()
    {
        return true;
    }
    public float getWeight()
    {
        return 0.0f;
    }
    public bool isSuccess()
    {
        return true;
    }
    public void execute()
    {

    }
}