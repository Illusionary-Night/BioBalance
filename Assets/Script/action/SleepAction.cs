using System.Collections.Generic;
using UnityEngine;

public class SleepAction : ActionBase
{
    public override ActionType Type => ActionType.Sleep;

    public override bool IsConditionMet(Creature creature)
    {
        int sleepHead = 900;
        int sleepTail = 1800;
        int nowHour = Manager.Instance.TickManager.CurrentHour;
        
        if (sleepHead > sleepTail) 
        {
            return nowHour >= sleepHead || nowHour < sleepTail;
        } 
        else 
        {
            return nowHour >= sleepHead && nowHour < sleepTail;
        }
    }

    public override float GetWeight(Creature creature)
    {
        //return 2f;

        int sleepHead = 900;
        int sleepTail = 1800;
        int sleepDuration = 900;
        int nowHour = Manager.Instance.TickManager.CurrentHour;
        int hoursPerDay = constantData.HOURS_PER_DAY;

        if (sleepDuration == 0) return 0f;
        
        // �p����e�ɶ��b�ίv�϶�������m
        int hoursIntoSleep = sleepHead <= sleepTail
            ? nowHour - sleepHead
            : (nowHour >= sleepHead ? nowHour - sleepHead : hoursPerDay - sleepHead + nowHour);

        // ���W�ƶi�� (0.0 ~ 1.0)
        float progress = (float)hoursIntoSleep / sleepDuration;

        // �������u�G�����ɬq�v���̰�
        float weight = Mathf.Sin(Mathf.PI * progress);

        const float minWeight = 0.9f;
        const float maxWeight = 2.0f;
        return Mathf.Lerp(minWeight, maxWeight, weight);
    }

    public override bool IsSuccess(Creature creature)
    {
        return true;
    }

    public override void Execute(Creature creature, ActionContext context = null)
    {
        creature.SetMovementState(CreatureMovementState.Sleep);
        context?.Complete();
    }


}
