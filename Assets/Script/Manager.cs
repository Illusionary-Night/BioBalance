using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    private List<Creature> creatures;
    private List<GameObject> meat;
    private List<GameObject> grass;
    private List<CreatureAttributes> species;
    public float tickInterval = 1f; // 每個遊戲單位時間 = 1 秒
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
            OnTick(); // 觸發一個遊戲時間單位
        }
    }
    private void OnTick()
    {
        // 在這裡處理每個遊戲時間單位的邏輯
        foreach (var creature in creatures)
        {
            // 更新生物的狀態與行為
            creature.UpdateState();
        }
    }
    private void PredatorUpdate(Creature new_creature)
    {
        for (int i = 0; i < creatures.Count; i++)       //檢查現有生物列表
        {
            for (int j = 0; j < creatures[i].FoodList.Count; j++)       //檢查現有生物的食物列表
            {
                if (creatures[i].FoodList[j].GetType() == (new_creature.GetType()))     // new_creature 是 creatures[i] 的食物
                {
                    // creatures[i] 會獵捕 new_creature
                    if (!new_creature.PredatorList.Contains(creatures[i]))      //避免重複加入
                    {
                        new_creature.PredatorList.Add(creatures[i]);        //加入天敵列表
                    }
                }
            }
        }
        for (int i = 0; i < new_creature.FoodList.Count; i++)    //檢查 new_creature 的食物列表
        {
            for (int j = 0; j < creatures.Count; j++)            //檢查現有生物列表
            {
                if (creatures[j].GetType() == (new_creature.FoodList[i].GetType()))     // creatures[j] 是 new_creature 的食物
                {
                    // new_creature 會獵捕 creatures[j]
                    if (!creatures[j].PredatorList.Contains(new_creature))      //避免重複加入
                    {
                        creatures[j].PredatorList.Add(new_creature);        //加入天敵列表
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
