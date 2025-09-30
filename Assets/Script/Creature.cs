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
    Herbivore,  //��
    Carnivore,  //�׭�
    Omnivore    //����
}
public abstract class creature
{
    // ���a�M�w
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

    // �q���p��
    public float HungerRate { get; set; }
    public float MaxHunger { get; set; }
    public float Lifespan { get; set; }
    public float ReproductionInterval { get; set; }

    // ��e���A
    public float Hunger { get; set; }
    public float Health { get; set; }
    public float Age { get; set; }
    public float ReproductionCooldown { get; set; }
    // Start is called before the first frame update
    public void action()
    {
        List<KeyValuePair<action,float>> availableActions = new List<KeyValuePair<action,float>>();
        //�C�^�X�}�l(�C�ͪ��y�{)	
        //�p��C��action������F���P�_
        //�p��C�ӹF������action���v��
        for (int i = 0; i < ActionList.Count; i++)
        {
            if (ActionList[i].isConditionMet())
            {
                availableActions.Add(new KeyValuePair<action,float>(ActionList[i], ActionList[i].getWeight()));
            }
        }
        //�N����F����action�i���v���Ƨ�
        availableActions.Sort((x, y) => y.Value.CompareTo(x.Value));
        for (int i = 0; i < availableActions.Count; i++)
        {
            //����v���̰�
            action selectedAction = availableActions[0].Key;
            //�릨�\�v
            if (selectedAction.isSuccess())
            {
                selectedAction.execute();
                break;
            }
            else
            {
                //����    ���v������
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