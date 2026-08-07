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

    //TODO: 數值的合不合法交給data自行判斷即可
    //TODO: set數值的部分，不確定該怎麼set


    // /// <summary> 立即設置當前生命值，並確保在合法範圍內 </summary>
    // public void SetHealth(float value)
    // {
    //     health = Mathf.Clamp(value, 0, maxHealth);
    // }

    // /// <summary> 立即設置當前年齡，並確保不超過壽命上限 </summary>
    // public void SetAge(float value)
    // {
    //     age = Mathf.Clamp(value, 0, lifespan);
    // }



    // /// <summary> 設置是否進入無敵狀態（用於 Debug 或特殊事件，不會死亡） </summary>
    // public void SetInvincible(bool isInvincible)
    // {
    //     data.isInvincible = isInvincible;
    // }



    /// <summary>
    /// 讓生物進入暈眩狀態
    /// </summary>
    /// <param name="duration">暈眩持續的時間 (單位：Tick)</param>
    // public void SetStun(float duration)
    // {
    //     // 如果已經在暈眩中，可以選擇「取最大值」或「疊加」
    //     data.stunTimer = Mathf.Max(data.stunTimer, duration);

    // }

    #endregion

    #region --- 動作與冷卻管理 ---

    /// <summary> 取得當前運作中的狀態機實例 </summary>
    // public ActionStateMachine GetStateMachine()
    // {
    //     return data.actionStateMachine;
    // }

    /// <summary> 記錄目前正在執行的 Action 類型，供觀察者或 UI 顯示 </summary>
    public void SetCurrentAction(ActionType type)
    {
        data.currentAction = type;
    }

    /// <summary> 重置所有動作冷卻（含通用 CD 與特定動作 CD 字典） </summary>
    public void ResetAllCooldowns()
    {
        data.actionCooldown = 0;
        // 清空字典中的冷卻
        var keys = new List<ActionType>(data.actionCD.Keys);
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


    #region --- 資料轉換與系統重置 ---
    /// <summary> 將當前個體的遺傳屬性轉換為屬性結構，供繁殖或保存使用 </summary>
    public CreatureAttributes ToCreatureAttribute()
    {
        CreatureAttributes attributes = new CreatureAttributes();
        attributes.size = data.size;
        attributes.max_health = data.health.maxHealth;
        attributes.speed = data.speed;
        attributes.attack_power = attackPower;
        attributes.reproduction_rate = data.reproductionRate;
        attributes.lifespan = data.age.maxAge;
        attributes.perception_range = data.perceptionRange;
        attributes.gender = gender;
        attributes.UUID = data.UUID;
        attributes.colorGenes = colorGenes;
        return attributes;
    }
    //TODO: 感覺有點怪怪的，reset不是這樣吧？
    /// <summary> 重置生物狀態（供物件池重用時調用）/// </summary>
    public void ResetState()
    {
        data.isDead = false;
        data.isInvincible = false;
        data.underAttackDirection = Direction.None;

        // 重置狀態機
        data.actionStateMachine = null;
        movement = null;

        // 重置冷卻
        ResetAllCooldowns();
    }
    #endregion



    #region 尚未歸類function 
    public void SetMotherID(string motherID)
    {
        this.motherID = motherID;
    }
    public void SetFatherID(string fatherID)
    {
        this.fatherID = fatherID;
    }
    //TODO: 優化，不要用Collider2D的半徑來判斷距離，改用size的某種配方，讓設計師可以調整生物之間的互動範圍。
    public bool IsNearby(Creature another)
    {
        Vector2 position1 = this.transform.position;
        Vector2 position2 = another.transform.position;
        float DistanceGate = GetContactDistance(another);
        if (DistanceGate < Vector2.Distance(position1, position2)) return false;
        Debug.Log("is nearby");
        return true;
    }
    public float GetContactDistance(Creature another)
    {
        // 1. 去抓 Unity 物理引擎身上那個真實的 CircleCollider2D
        var myCol = this.GetComponent<CircleCollider2D>();
        var otherCol = another.GetComponent<CircleCollider2D>();

        if (myCol != null && otherCol != null)
        {
            // 2. 讓程式碼承認物理引擎的半徑 (就是抓出來的值)
            float myRealRadius = myCol.radius * this.transform.localScale.x;
            float otherRealRadius = otherCol.radius * another.transform.localScale.x;

            // 3. 回傳真實的極限距離再乘以 1.2 容差
            return (myRealRadius + otherRealRadius) * 1.2f;
        }

        // 防呆機制：萬一沒掛碰撞體才用舊的方法
        return (this.transform.localScale.x + another.transform.localScale.x) * 0.5f * 1.2f;
    }
    /// <summary>
    /// 判斷生物是否處於發情狀態，根據年齡、飢餓度、健康狀態等多重條件綜合評估。只有當生物達到成年、身體狀況良好且沒有處於冷卻期時，才會返回 true，表示可以進行繁殖行為。
    /// </summary>
    /// <returns></returns>
    public bool IsInHeat()
    {
        // 1. 年齡門檻：必須是成年體 (假設你有 lifespan 或 matureAge 的設定)
        // 這裡假設超過壽命的 20% 才算成年
        if (data.age.Percentage < 0.2f)
            return false;

        // 2. 老年停經 (選配)：如果太老了也不生了，避免佔用資源
        if (data.age.Percentage > 0.9f)
            return false;

        // 3. 能量門檻：肚子至少要有 60% 飽，才有體力繁衍
        if (data.hunger.Percentage < 0.6f)
            return false;

        // 4. 健康門檻 (選配)：如果快死了 (血量低於 30%)，優先保命不發情
        if (data.health.Percentage < 0.3f)
            return false;

        // 5. 冷卻期檢查：(針對雌性產後的 CD 時間，假設你有個計時器 matingCooldown)
        //if (c.matingCooldown > 0f)
        //    return false;

        // 如果以上都通過了，代表生理狀態極佳，可以繁衍！
        return true;
    }
    /// <summary>
    /// 判斷附近是否有處於發情狀態的雄性生物
    /// </summary>
    /// <param name="c">要檢查的生物</param>
    /// <returns></returns>
    public bool IsInHeatMaleNearby(Creature c)
    {
        if (c.gender == Gender.Female) return false;
        if (!c.IsInHeat()) return false;
        if (!this.IsNearby(c)) return false;
        return true;
    }
    public bool IsInHeatFemale(Creature c)
    {
        if (c.gender == Gender.Male) return false;
        if (!c.IsInHeat()) return false;
        return true;
    }
    #endregion



}


