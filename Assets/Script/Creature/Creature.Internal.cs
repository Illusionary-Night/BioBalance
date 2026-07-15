using System.Linq;
using UnityEngine;

public partial class Creature : MonoBehaviour
{

    // 這邊的想法是先執行減益類型的更新，然後進行死亡判定，再進行增益類型的更新。
    // 這樣能避免出現數值到邊界卻不死或是在死線反覆橫跳的情況。
    // 有更好的解決方案再說。
    private void UpdateVitalSigns()
    {
        // 飢餓處理
        data.hunger.Add(-GetCurrentHungerDrain());

        // 老化處理
        data.age.Add(1);

        // 死亡判定
        if (!data.isInvincible && (data.health.Percentage <= 0 || data.hunger.Percentage <= 0 || data.age.Percentage >= 1)) Die();

        // 回血處理
        data.health.Add(data.healthRegeneration);
    }
    private void UpdateCooldowns()
    {
        //行動冷卻
        if (data.actionCooldown > 0)
        {
            data.actionCooldown -= 1;
        }

        foreach (var key in data.actionCD.Keys.ToList())
        {
            if (data.actionCD[key] > 0)
            {
                data.actionCD[key] -= 1;
            }
        }
        //TODO: ActionAttr處理
        if (reproductionCD > 0)
        {
            reproductionCD -= 1;
        }
        //----------------------------------------------
        if (data.stunTimer > 0)
        {
            data.isStunned = true;
            data.stunTimer--;
        }
        else
        {
            data.isStunned = false;
        }
    }


    private void UpdateGrowth()
    {
        if (data.isDead) return;

        // --- 1. 更新 LifeState (基於年齡百分比) ---
        float lifePercentage = data.age.Percentage;
        UpdateLifeState(lifePercentage);

        // --- 2. 執行視覺成長 (假設幼體從基因 size 的 60% 長到 100%) ---
        float growthMultiplier = Mathf.Lerp(0.6f, 1.0f, Mathf.Min(lifePercentage * 2f, 1.0f));
        float currentAbsoluteSize = data.size * growthMultiplier;

        float finalScale = currentAbsoluteSize * constantData.NORMAL_SIZE;
        transform.localScale = new Vector3(finalScale, finalScale, 1f);

        // --- 3. 更新 BodyType (基於絕對大小) ---
        UpdateBodyType(currentAbsoluteSize);
    }

    private void UpdateLifeState(float lifePercentage)
    {
        if (lifePercentage < 0.15f) data.currentLifeState = LifeState.Infant;
        else if (lifePercentage < 0.4f) data.currentLifeState = LifeState.Juvenile;
        else if (lifePercentage < 0.85f) data.currentLifeState = LifeState.Adult;
        else data.currentLifeState = LifeState.Elder;
    }

    private void UpdateBodyType(float currentSize)
    {
        // 這裡的閾值（1.5, 4.0）應根據你遊戲中的生物平均大小設定
        if (currentSize < 1.5f) data.currentBodyType = BodyType.Small;
        else if (currentSize < 4.0f) data.currentBodyType = BodyType.Medium;
        else data.currentBodyType = BodyType.Large;
    }

    /// <summary>
    /// 計算當前狀態下的飢餓消耗量
    /// </summary>
    /// <returns>回傳每秒消耗的飢餓值</returns>
    public float GetCurrentHungerDrain()
    {
        float multiplier = 1f;
        // TODO: 之後可以把multiplier的get set 調成屬性，方便Action直接調整。
        // 1. 根據移動狀態決定倍率
        switch (data.movementState)
        {
            case CreatureMovementState.Sleep:
                multiplier = 0.5f;
                break;
            case CreatureMovementState.Idle:
                multiplier = 0.8f;
                break;
            case CreatureMovementState.Walk:
                multiplier = 1.0f;
                break;
            case CreatureMovementState.Run:
                multiplier = 2.0f;
                break;
            case CreatureMovementState.Stunned:
                multiplier = 1.2f;
                break;
            default:
                multiplier = 1.0f;
                break;
        }

        return data.hunger.hungerRate * multiplier;
    }

    /// <summary>
    ///  初始化生物的運行時狀態，確保每次生成或重置時都回到初始狀態。
    /// </summary>
    private void ResetRuntimeStates()
    {
        //初始狀態
        // TODO: 初始化需要裝一個新的嗎？
        data.hunger. = data.maxHunger;
        data.health = data.maxHealth;
        data.age = 0;
        data.actionCooldown = 0;
        data.reproductionCD = 0;
        data.stunTimer = 0f;
        data.actionCD.Clear();

        data.isDead = false;
        data.isSleeping = false;
        data.isStunned = false;
        data.isInvincible = false;

        //matingPartner = null;
        data.enemy = null;
        data.fatherID = string.Empty;
        data.motherID = string.Empty;
        data.currentLifeState = LifeState.Infant;
        //currentBodyType = BodyType
        data.movementState = CreatureMovementState.Idle;
        data.underAttackDirection = Direction.None;

    }




}

