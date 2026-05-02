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
    }




    private void SetCreatureSprite(CreatureBase baseType)
    {
        // 1. 將 Enum 轉為字串 (例如 "Slime")
        string spriteName = baseType.ToString();

        // 2. 從 Resources 加載 (路徑需放在 Resources/Sprites/ 下)
        Sprite loadedSprite = Resources.Load<Sprite>($"Sprites/{spriteName}");

        if (loadedSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = loadedSprite;

            var col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                // 讓它自動抓！
                // 取圖片寬高之中較大的一半作為半徑，完美貼合任何形狀的生物圖片
                float maxDim = Mathf.Max(loadedSprite.bounds.size.x, loadedSprite.bounds.size.y);
                col.radius = maxDim * 0.5f;
            }
        }
        else
        {
            Debug.LogError($"找不到對應圖片: Sprites/{spriteName}");
        }
    }
    private void AutoSetLayer(GameObject obj)
    {
        // 將名稱轉為索引編號 (例如 "Creature" 是第 6 層，layerIndex 就會是 6)
        int layerIndex = LayerMask.NameToLayer("Creature");

        if (layerIndex == -1)
        {
            Debug.LogError("找不到名為 'Creature' 的 Layer，請先在選單中手動建立！");
            return;
        }

        // 設置該物件及其所有子物件的 Layer
        SetLayerRecursive(obj, layerIndex);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }

    private void AttributeInheritance(Species species, CreatureAttributes? parentAttr1 = null, CreatureAttributes? parentAttr2 = null)
    {
        // 變異引擎：生出一個介於 -variation 到 +variation 之間的隨機比例
        float variationFactor() => UnityEngine.Random.Range(-species.variation, species.variation);

        // 核心遺傳與變異邏輯 (完美包容單親與雙親)
        float Inherit(float speciesVal, float? parent1Val, float? parent2Val)
        {
            float baseValue = 0;
            if (!parentAttr1.HasValue && !parentAttr2.HasValue)
            {
                //初代個體
                baseValue = speciesVal;
            }
            else if (!parentAttr2.HasValue)
            {
                // 【無性生殖/孤雌生殖】：直接拿單親的屬性
                baseValue = parent1Val.Value;
            }
            else
            {
                // 【有性生殖】：擲骰子決定拿爸爸還是媽媽的數值
                baseValue = UnityEngine.Random.value > 0.5f ? parent1Val.Value : parent2Val.Value;
            }

            // 不管是有性還是無性，最後統統乘上變異係數
            return baseValue + (baseValue * variationFactor());
        }

        ////睡眠時間變異-------------------------這個東西處理比較麻煩
        //int delta_sleep_time() => (int)((creatureAttributes.sleeping_tail - creatureAttributes.sleeping_head) * variationFactor());
        //sleepingHead = creatureAttributes.sleeping_head + delta_sleep_time();
        //sleepingTail = creatureAttributes.sleeping_tail + delta_sleep_time();
        //其他玩家屬性變異
        size = Inherit(species.baseSize, parentAttr1?.size, parentAttr2?.size);
        speed = Inherit(species.baseSpeed, parentAttr1?.speed, parentAttr2?.speed);
        maxHealth = Inherit(species.baseMaxHealth, parentAttr1?.max_health, parentAttr2?.max_health);
        reproductionRate = Inherit(species.baseReproductionRate, parentAttr1?.reproduction_rate, parentAttr2?.reproduction_rate);
        attackPower = Inherit(species.baseAttackPower, parentAttr1?.attack_power, parentAttr2?.attack_power);
        lifespan = Inherit(species.baseLifespan, parentAttr1?.lifespan, parentAttr2?.lifespan);
        perceptionRange = Inherit(species.basePerceptionRange, parentAttr1?.perception_range, parentAttr2?.perception_range);

        if (!parentAttr1.HasValue && !parentAttr2.HasValue)
        {
            //初代個體
            if (species.reproductionType == ReproductionType.Asexual)
            {
                gender = Gender.None;
            }
            else
            {
                gender = UnityEngine.Random.value > 0.5f ? Gender.Male : Gender.Female;
            }
        }
        else if (!parentAttr2.HasValue)
        {
            // 【無性生殖/孤雌生殖】：直接拿單親的屬性
            gender = parentAttr1?.gender ?? Gender.None;
        }
        else
        {
            // 【有性生殖】：擲骰子決定拿爸爸還是媽媽的數值
            gender = UnityEngine.Random.value > 0.5f ? Gender.Male : Gender.Female;
        }

        //計算衍生屬性
        //sleepTime = sleepingTail - sleepingHead;
        hungerRate = AttributesCalculator.CalculateHungerRate(size, speed, attackPower);
        maxHunger = AttributesCalculator.CalculateMaxHunger(size, maxHealth, foodTypes);
        healthRegeneration = AttributesCalculator.CalculateHealthRegeneration(maxHealth, size);
        
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

    private void ResetRuntimeStates() {
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