/*
 * [區段名稱] Hurt Section
 * [區段說明] 負責 Creature 的生命值管理與受擊方向偵測。
 * [主要功能] 接收傷害數值、將攻擊者座標轉換為 8 向受擊方位、提供受擊狀態查詢。
 * [可用函式] void Hurt(int), void Hurt(int, Vector2), bool UnderAttack(), Direction GetUnderAttackDirection(), Direction GetAndResetDirection()
 * [測試區域] Inspector中的Debug Tools有拉桿可以設定受到攻擊的方向。
 */
using System.Collections.Generic;
using UnityEngine;

public partial class Creature : MonoBehaviour
{
    #region --- 生命狀態控制 ---

    /// <summary> 立即設置當前飢餓值，並確保在合法範圍內 </summary>
    public void SetHunger(float value)
    {
        hunger = Mathf.Clamp(value, 0, maxHunger);
    }

    /// <summary> 立即設置當前生命值，並確保在合法範圍內 </summary>
    public void SetHealth(float value)
    {
        health = Mathf.Clamp(value, 0, maxHealth);
    }

    /// <summary> 立即設置當前年齡，並確保不超過壽命上限 </summary>
    public void SetAge(float value)
    {
        age = Mathf.Clamp(value, 0, lifespan);
    }

    /// <summary> 恢復飽食度，增加數值並限制在 maxHunger 以內 </summary>
    public void RestoreHunger(float nutritionalValue)
    {
        hunger = Mathf.Min(hunger + nutritionalValue, maxHunger);
    }

    /// <summary> 設置是否進入無敵狀態（用於 Debug 或特殊事件，不會死亡） </summary>
    public void SetInvincible(bool isInvincible)
    {
        this.isInvincible = isInvincible;
    }

    /// <summary> 啟動睡眠狀態：觸發數值加成開關（飢餓消耗降低、回血提高） </summary>
    public void StartSleeping()
    {
        isSleeping = true;
        // 這裡可以觸發動畫或視覺效果
    }

    /// <summary> 結束睡眠狀態：恢復常規數值消耗比例 </summary>
    public void StopSleeping()
    {
        isSleeping = false;
    }
    #endregion

    #region --- 動作與冷卻管理 ---

    /// <summary> 取得當前運作中的狀態機實例 </summary>
    public ActionStateMachine GetStateMachine()
    {
        return actionStateMachine;
    }

    /// <summary> 記錄目前正在執行的 Action 類型，供觀察者或 UI 顯示 </summary>
    public void SetCurrentAction(ActionType type)
    {
        currentAction = type;
    }

    /// <summary> 重置所有動作冷卻（含通用 CD 與特定動作 CD 字典） </summary>
    public void ResetAllCooldowns()
    {
        actionCooldown = 0;
        // 清空字典中的冷卻
        var keys = new List<ActionType>(actionCD.Keys);
        foreach (var key in keys) actionCD[key] = 0;
    }

    /// <summary> 觸發特定動作的冷卻計時。若 ScriptableObject 沒設定 CD 則會給予警告 </summary>
    public void ResetActionCooldown(ActionType actionType)
    {
        if (isDead) return;

        if (actionMaxCD.TryGetValue(actionType, out int maxCD))
        {
            actionCD[actionType] = maxCD;
        }
        else
        {
            // 如果開發者在編輯器沒設定 CD，給予警告並設為預設值 0，程式才不會斷掉
            Debug.LogWarning($"[Creature] {mySpecies.name} 缺少動作 {actionType} 的 CD 設定！");
            actionCD[actionType] = 0;
        }

        actionCooldown = constantData.UNIVERSAL_ACTION_COOLDOWN;
    }

    /// <summary> 查詢特定動作剩餘的冷卻時間（Ticks） </summary>
    public int GetActionCooldown(ActionType actionType)
    {
        if (actionCD.ContainsKey(actionType))
        {
            return actionCD[actionType];
        }
        return 0;
    }

    /// <summary> 查詢特定動作在該物種設定中的最大冷卻時間 </summary>
    public int GetMaxActionCooldown(ActionType actionType)
    {
        if (actionMaxCD.ContainsKey(actionType))
        {
            return actionMaxCD[actionType];
        }
        return 0;
    }

    /// <summary> 取得完整的剩餘冷卻字典 </summary>
    public Dictionary<ActionType, int> GetActionCDList()
    {
        return actionCD;
    }

    /// <summary> 取得物種預設的最大冷卻字典 </summary>
    public Dictionary<ActionType, int> GetActionMaxCDList()
    {
        return actionMaxCD;
    }
    #endregion

    #region --- 受擊系統 ---

    /// <summary> 執行基礎傷害扣血，並確保生命值不低於 0 </summary>
    public void Hurt(float damage)
    {
        underAttackDirection = Direction.None;
        health -= damage;
        health = Mathf.Max(health, 0);
    }

    /// <summary> 執行傷害並記錄攻擊來源方位，用於觸發受傷逃跑判定或者之後進一步的動畫或特效 </summary>
    public void Hurt(float damage, Vector2 attackerPosition)
    {
        // 計算攻擊者相對於自己的方位向量
        Vector2 direction = attackerPosition - (Vector2)transform.position;
        underAttackDirection = GetDirectionFromVector(direction);
        health -= damage;
        health = Mathf.Max(health, 0);
    }

    /// <summary> 將向量轉換為 8 方向列舉，以 45 度角為一個判斷區間 </summary>
    private Direction GetDirectionFromVector(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return Direction.None;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        if (angle >= 337.5f || angle < 22.5f) return Direction.East;
        if (angle >= 22.5f && angle < 67.5f) return Direction.Northeast;
        if (angle >= 67.5f && angle < 112.5f) return Direction.North;
        if (angle >= 112.5f && angle < 157.5f) return Direction.Northwest;
        if (angle >= 157.5f && angle < 202.5f) return Direction.West;
        if (angle >= 202.5f && angle < 247.5f) return Direction.Southwest;
        if (angle >= 247.5f && angle < 292.5f) return Direction.South;
        if (angle >= 292.5f && angle < 337.5f) return Direction.Southeast;

        return Direction.None;
    }

    /// <summary> 檢查目前是否處於受擊狀態（方位不為 None 代表受擊中） </summary>
    public bool UnderAttack()
    {
        return underAttackDirection != Direction.None;
    }

    /// <summary> 取得受擊方位</summary>
    public Direction GetUnderAttackDirection()
    {
        return underAttackDirection;
    }

    /// <summary> 取得受擊方位並立刻重置狀態，確保單次受傷僅觸發一次反應 </summary>
    public Direction GetAndResetUnderAttackDirection()
    {
        Direction result = underAttackDirection;
        underAttackDirection = Direction.None;
        return result;
    }
    #endregion

    #region --- 資料轉換與系統重置 ---
    /// <summary> 將當前個體的遺傳屬性轉換為屬性結構，供繁殖或保存使用 </summary>
    public CreatureAttributes ToCreatureAttribute()
    {
        CreatureAttributes attributes = new CreatureAttributes();
        attributes.size = size;
        attributes.max_health = maxHealth;
        attributes.speed = speed;
        attributes.attack_power = attackPower;
        attributes.reproduction_rate = reproductionRate;
        attributes.lifespan = lifespan;
        attributes.perception_range = perceptionRange;
        attributes.sleeping_head = sleepingHead;
        attributes.sleeping_tail = sleepingTail;
        return attributes;
    }


    /// <summary> 重置生物狀態（供物件池重用時調用）/// </summary>
    public void ResetState()
    {
        isDead = false;
        isInvincible = false;
        underAttackDirection = Direction.None;

        // 重置狀態機
        actionStateMachine = null;
        movement = null;

        // 重置冷卻
        ResetAllCooldowns();
    }
    #endregion

    #region --- 移動控制與導航 ---
    /// <summary> 指令生物移動至目標格座座標。會自動觸發尋路與物理移動 </summary>
    /// <param name="destination"> 目標格座 (Vector2Int) </param>
    public void MoveTo(Vector2Int destination)
    {
        if (isDead || movement == null) return;
        movement.SetDestination(destination);
    }

    /// <summary> 強制重新執行尋路演算法（A*）。用於目標位置改變或環境障礙物更新時 </summary>
    public void ForceNavigate()
    {
        if (isDead || movement == null) return;
        movement.Navigate();
    }

    #endregion

    #region --- 空間與位置查詢  ---

    /// <summary> 取得當前經過物理修正後的整數格座標（四捨五入） </summary>
    /// <returns> 當前所處的格座位置 </returns>
    public Vector2Int GetRoundedPosition()
    {
        if (movement == null) return Vector2Int.zero;
        return movement.GetVector2IntCurrentPosition();
    }

    /// <summary> 計算當前位置與導航目的地之間的直線距離。若無目的地則回傳 -1 </summary>
    public float GetDistanceToDestination()
    {
        // 如果 movement 沒啟動、或是沒在動，就回傳 -1
        if (movement == null || movement.GetDestination() == null)
        {
            return -1f;
        }
        Vector2 currentPos = transform.position;
        Vector2 dest = new Vector2(movement.GetDestination().x, movement.GetDestination().y);

        return Vector2.Distance(currentPos, dest);
    }

    /// <summary> 取得當前移動組件預設的目的地座標 </summary>
    public Vector2Int GetMovementDestination()
    {
        return movement != null ? movement.GetDestination() : Vector2Int.zero;
    }

    /// <summary> 取得生物目前在移動過程中被障礙物卡住的累計次數（Ticks） </summary>
    /// <returns> 卡住的次數，可用於判定是否需要重新導航或變換行為 </returns>
    public int GetMovementStuckTimes()
    {
        return movement != null ? movement.GetStuckTimes() : 0;
    }
    #endregion
}
