using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveAction : ActionBase
{
    public override ActionType Type => ActionType.Move;
    //public override int Cooldown => 10;
    //[SerializeField] private static readonly int MoveDistance = 100;

    public override bool IsConditionMet(Creature creature)
    {
        // 永遠可以移動
        return true;
    }

    public override float GetWeight(Creature creature)
    {
        return 0.9f;
        // 根據飢餓值決定移動權重，飢餓值越高，移動權重越高
        //return (creature.Hunger / creature.MaxHunger) / Perception.Creatures.CountTargetNumber(creature, creature.SpeciesID);
    }

    public override bool IsSuccess(Creature creature)
    {
        //return true;
        // 1/2 機率成功移動
        return Random.Range(0, 2) == 0;
    }

    public override void Execute(Creature creature, ActionContext context = null)
    {
        //Debug.Log("move");
        // 隨機移動到鄰近位置
        Vector2Int currentPosition = creature.GetRoundedPosition();
        int rangeInt = Mathf.FloorToInt(creature.PerceptionRange);
        Vector2Int randomDisplacement = new(Random.Range(-rangeInt, rangeInt + 1), Random.Range(-rangeInt, rangeInt + 1));
        //Vector2Int randomDisplacement = new(Random.Range(-MoveDistance, MoveDistance + 1), Random.Range(-MoveDistance, MoveDistance + 1));
        Vector2Int newPosition = currentPosition + randomDisplacement;

        // 呼叫Creature自行導航地點
        creature.MoveTo(newPosition);
    }
    //private void Execute_type2(Creature creature, ActionContext context = null)
    //{
    //    Vector2Int currentPos = creature.GetRoundedPosition();
    //    float range = creature.PerceptionRange;

    //    // 1. 取得同族清單
    //    List<Creature> kindreds = Perception.Creatures.GetAllTargets(creature, creature.SpeciesID);

    //    // 如果周圍沒人，就隨機走
    //    if (kindreds == null || kindreds.Count == 0)
    //    {
    //        int rangeInt = Mathf.FloorToInt(creature.PerceptionRange);
    //        Vector2Int randomDisplacement = new(Random.Range(-rangeInt, rangeInt + 1), Random.Range(-rangeInt, rangeInt + 1));
    //        //Vector2Int randomDisplacement = new(Random.Range(-MoveDistance, MoveDistance + 1), Random.Range(-MoveDistance, MoveDistance + 1));
    //        Vector2Int newPosition = currentPos + randomDisplacement;

    //        // 呼叫Creature自行導航地點
    //        creature.MoveTo(newPosition);
    //        return;
    //    }

    //    Vector2Int bestPosition = currentPos;
    //    float lowestCrowdScore = float.MaxValue;

    //    // 2. 多點採樣（例如抽 8 個點來評比）
    //    int samples = 8;
    //    for (int i = 0; i < samples; i++)
    //    {
    //        Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * range;
    //        Vector2Int candidatePos = currentPos + Vector2Int.RoundToInt(randomPoint);

    //        // 3. 計算該點的擁擠分數 (使用 LINQ 快速計算)
    //        // 分數 = 總和( 1 / 與每個同族的距離 )
    //        float crowdScore = kindreds.Sum(k =>
    //        {
    //            float dist = Vector2.Distance(candidatePos, k?.transform.position??Vector3.zero);
    //            // 避免除以 0，並設定一個感應閾值（例如距離 0.5 內視為極度擁擠）
    //            return 1.0f / Mathf.Max(dist, 0.5f);
    //        });

    //        // 加入一點隨機擾動，讓行為更自然
    //        crowdScore *= UnityEngine.Random.Range(0.8f, 1.2f);

    //        if (crowdScore < lowestCrowdScore)
    //        {
    //            lowestCrowdScore = crowdScore;
    //            bestPosition = candidatePos;
    //        }
    //    }

    //    // 4. 前往人最少的地方
    //    creature.MoveTo(bestPosition);
    //}
}