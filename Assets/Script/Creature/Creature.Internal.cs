using System.Linq;
using UnityEngine;

public partial class Creature : MonoBehaviour
{
    private void UpdateVitalSigns()
    {
        // 飢餓處理
        hunger = Mathf.Clamp(hunger - GetCurrentHungerDrain(), 0, maxHunger);

        // 回血處理
        float currentRegen = isSleeping ? healthRegeneration * 2.0f : healthRegeneration;
        if (health > 0) health = Mathf.Min(health + currentRegen, maxHealth);

        // 老化處理
        age = Mathf.Min(age + 1, lifespan);

        // 死亡判定
        if (!isInvincible && (health <= 0 || hunger <= 0 || age >= lifespan)) Die();
    }
    private void UpdateCooldowns()
    {
        //行動冷卻
        if (actionCooldown > 0)
        {
            actionCooldown -= 1;
        }

        foreach (var key in actionCD.Keys.ToList())
        {
            if (actionCD[key] > 0)
            {
                actionCD[key] -= 1;
            }
        }
        //TODO: 理論上可以被ActionData解決掉
        if (reproductionCD > 0)
        {
            reproductionCD -= 1;
        }
        if (stunTimer > 0)
        {
            isStunned = true;
            stunTimer--;
        }
        else
        {
            isStunned = false;
        }
    }


    private void UpdateGrowth()
    {
        if (isDead) return;

        // --- 1. 更新 LifeState (基於年齡百分比) ---
        float lifePercentage = age / lifespan;
        UpdateLifeState(lifePercentage);

        // --- 2. 執行視覺成長 (假設幼體從基因 size 的 60% 長到 100%) ---
        float growthMultiplier = Mathf.Lerp(0.6f, 1.0f, Mathf.Min(lifePercentage * 2f, 1.0f));
        float currentAbsoluteSize = size * growthMultiplier;

        float finalScale = currentAbsoluteSize * constantData.NORMAL_SIZE;
        transform.localScale = new Vector3(finalScale, finalScale, 1f);

        // --- 3. 更新 BodyType (基於絕對大小) ---
        UpdateBodyType(currentAbsoluteSize);
    }

    private void UpdateLifeState(float lifePercentage)
    {
        if (lifePercentage < 0.15f) currentLifeState = LifeState.Infant;
        else if (lifePercentage < 0.4f) currentLifeState = LifeState.Juvenile;
        else if (lifePercentage < 0.85f) currentLifeState = LifeState.Adult;
        else currentLifeState = LifeState.Elder;
    }

    private void UpdateBodyType(float currentSize)
    {
        // 這裡的閾值（1.5, 4.0）應根據你遊戲中的生物平均大小設定
        if (currentSize < 1.5f) currentBodyType = BodyType.Small;
        else if (currentSize < 4.0f) currentBodyType = BodyType.Medium;
        else currentBodyType = BodyType.Large;
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
        switch (movementState)
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

        return hungerRate * multiplier;
    }

    /// <summary>
    ///  初始化生物的運行時狀態，確保每次生成或重置時都回到初始狀態。
    /// </summary>
    private void ResetRuntimeStates()
    {
        //初始狀態
        hunger = maxHunger;
        health = maxHealth;
        age = 0;
        actionCooldown = 0;
        reproductionCD = 0;
        stunTimer = 0f;
        actionCD.Clear();

        isDead = false;
        isSleeping = false;
        isStunned = false;
        isInvincible = false;

        //matingPartner = null;
        enemy = null;
        fatherID = string.Empty;
        motherID = string.Empty;
        currentLifeState = LifeState.Infant;
        //currentBodyType = BodyType
        movementState = CreatureMovementState.Idle;
        underAttackDirection = Direction.None;

    }




}

