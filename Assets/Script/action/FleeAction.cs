using System.Collections.Generic;
using UnityEditorInternal.VR;
using UnityEngine;

public class FleeAction : ActionBase
{
    public FleeAction() { }

    public override ActionType Type => ActionType.Flee;
    public override int Cooldown => 3;
    [SerializeField] private static readonly int FleeRange = 5;
    [SerializeField] private static readonly int PredationRange = 5;
    public override bool IsConditionMet(Creature creature)
    {
        return Perception.HasTarget(creature, creature.PredatorIDList);   
    }

    public override float GetWeight(Creature creature)
    {
        //(1 / 偵查範圍內同類個體數 + 1) * 0.8
        return (1f / (Perception.CountTargetNumber(creature, creature.SpeciesID) + 1)) * 0.8f;
    }

    public override bool IsSuccess(Creature creature)
    {
        return Random.value < 0.6f; // 60% 成功率
    }

    public override void Execute(Creature creature)
    {
        //// 前提有一個能跑動的範圍距離
        //Vector2Int currentPosition = this.TempGetCurrentPosition(creature);
        //// 遍歷天敵物種 遍歷天敵個體
        //// 以天敵分布為參考 將與天敵的距離平方反比作為權重繪製安全地帶圖 與天敵距離超過感知範圍則不考慮
        //Dictionary<Vector2Int, float> dangerMap = new();
        //foreach (var predator_ID in creature.PredatorIDList)
        //{
        //    foreach(var each_predator in Manager.species_dictionary[predator_ID].creatures)
        //    {
        //        Vector2Int predatorPosition = this.TempGetCurrentPosition(each_predator);
        //        float distance = Vector2Int.Distance(currentPosition, predatorPosition);
        //        if (distance > creature.PerceptionRange)continue;
        //        for(int i=-PredationRange; i<PredationRange; i++)
        //        {
        //            for(int j=-PredationRange; j<PredationRange; j++)
        //            {
        //                if (predatorPosition.x+i<0)
        //                {

        //                }
        //            }
        //        }
        //    }
        //}
        //// 列出前幾安全的地點 兩兩相近者排除 
        //// 將每塊地與最近天敵的距離反比作為權重合併地形權重圖，製成逃生路徑圖
        //// 依此地圖計算出前幾安全的地點各自路徑總花費
        //// 選擇最佳地點並獲得該路徑
        //// 後續處理....
    }
    //TODO: 移動相關的輔助方法
    private bool TempTransformPosition(List<Vector2Int> path)
    {
        // 在這裡添加位置轉換的邏輯
        return true;
    }
    private Vector2Int TempGetCurrentPosition(Creature creature)
    {
        // 在這裡添加獲取當前位置的邏輯
        return new Vector2Int(0, 0);
    }
}