using System.Collections.Generic;
using UnityEngine;

public class MoveAction : ActionBase
{
	public override ActionType Type => ActionType.Move;
	public override int Cooldown => 5;
	[SerializeField] private static readonly int MoveDistance = 5;

    public override bool IsConditionMet(Creature creature)
	{
		// 永遠可以移動
		return true;
	}

	public override float GetWeight(Creature creature)
	{
		// 根據飢餓值決定移動權重，飢餓值越高，移動權重越高
		return (creature.Hunger / creature.MaxHunger) / Perception.Creatures.CountTargetNumber(creature, creature.SpeciesID);
	}

	public override bool IsSuccess(Creature creature)
	{
        // 1/2 機率成功移動
        return Random.Range(0, 2) == 0;
	}

	public override void Execute(Creature creature)
	{
		// 隨機移動到鄰近位置
		Vector2Int currentPosition = this.TempGetCurrentPosition();

		Vector2Int randomDisplacement = new(Random.Range(-MoveDistance, MoveDistance + 1), Random.Range(-MoveDistance, MoveDistance + 1));
		Vector2Int newPosition = currentPosition + randomDisplacement;

        // 使用 A* 演算法尋找路徑
        List<Vector2Int> path = AStar.FindPath(currentPosition, newPosition, TerrainGenerator.Instance.GetDefinitionMap().GetTerrainWeight);

        // 更新生物位置
        this.TempTransformPosition(path);
    }


    //TODO: 移動相關的輔助方法
    private bool TempTransformPosition(List<Vector2Int> path)
	{
		// 在這裡添加位置轉換的邏輯
		return true;
	}
	private Vector2Int TempGetCurrentPosition()
	{
		// 在這裡添加獲取當前位置的邏輯
		return new Vector2Int(0, 0);
    }
}