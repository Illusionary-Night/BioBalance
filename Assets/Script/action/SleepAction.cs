using System.Collections.Generic;
using UnityEngine;

public class SleepAction : ActionBase
{
    public override ActionType Type => ActionType.Sleep;

    //private const float SleepHungerRateMultiplier = 0.3f;
    public override bool IsConditionMet(Creature creature)
    {
        int sleepHead = creature.sleepingHead;
        int sleepTail = creature.sleepingTail;
        int nowHour = TickManager.Instance.CurrentHour;
        
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

        int sleepHead = creature.sleepingHead;
        int sleepTail = creature.sleepingTail;
        int sleepDuration = creature.sleepTime;
        int nowHour = TickManager.Instance.CurrentHour;
        int hoursPerDay = constantData.HOURS_PER_DAY;

        if (sleepDuration == 0) return 0f;
        
        // 計算當前時間在睡眠區間中的位置
        int hoursIntoSleep = sleepHead <= sleepTail
            ? nowHour - sleepHead
            : (nowHour >= sleepHead ? nowHour - sleepHead : hoursPerDay - sleepHead + nowHour);

        // 正規化進度 (0.0 ~ 1.0)
        float progress = (float)hoursIntoSleep / sleepDuration;

        // 正弦曲線：中間時段權重最高
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
        creature.MoveTo(creature.GetRoundedPosition());

        context?.Complete();
        //Debug.Log($"{creature.name} start sleep. Now time：{TickManager.Instance.CurrentHour}:00");
    }


}
