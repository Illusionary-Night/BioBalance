using UnityEngine;

public class HurtSystem
{

    /// <summary> 執行基礎傷害扣血，並確保生命值不低於 0 </summary>
    public void Hurt(CreatureData data, float damage, Creature attacker = null)
    {
        data.underAttackDirection = Direction.None;
        data.health.Add(-damage);
        if (attacker != null) data.enemy = attacker;
    }

    /// <summary> 執行傷害並記錄攻擊來源方位，用於觸發受傷逃跑判定或者之後進一步的動畫或特效 </summary>
    /// <param name="data">當前個體</param>
    /// <param name="damage">受到傷害量</param>
    /// <param name="attackerPosition">受到傷害的方向aka敵人的方位</param>
    /// <param name="attacker">攻擊他的敵人</param>
    public void Hurt(CreatureData data, float damage, Vector2 attackerPosition, Creature attacker = null)
    {
        // 計算攻擊者相對於自己的方位向量
        Vector2 direction = attackerPosition - (Vector2)data.position;
        data.underAttackDirection = DirectionHelper.GetDirectionFromVector(direction);
        data.health.Add(-damage);
        if (attacker != null) data.enemy = attacker;
    }



    /// <summary> 檢查目前是否處於受擊狀態（方位不為 None 代表受擊中） </summary>
    public bool UnderAttack(CreatureData data)
    {
        return data.underAttackDirection != Direction.None;
    }

    /// <summary> 取得受擊方位</summary>
    public Direction GetUnderAttackDirection(CreatureData data)
    {
        return data.underAttackDirection;
    }

    /// <summary> 取得受擊方位並立刻重置狀態，確保單次受傷僅觸發一次反應 </summary>
    public Direction GetAndResetUnderAttackDirection(CreatureData data)
    {
        Direction result = data.underAttackDirection;
        data.underAttackDirection = Direction.None;
        return result;
    }
    /// <summary>
    /// 初始化受擊方位
    /// </summary>
    /// <param name="data">當前個體</param>
    public void ResetUnderAttackDirection(CreatureData data)
    {
        data.underAttackDirection = Direction.None;
    }
    // TODO: 這裡要轉接給另外一個tool感覺怪怪的？
    public void Repeled(Creature creature, Vector2 drection, float strength = 1f)
    {
        MovementSystem.Pushed(creature, drection, strength);
    }

}
