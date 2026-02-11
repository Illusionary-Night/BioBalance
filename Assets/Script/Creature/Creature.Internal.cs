using System.Linq;
using UnityEngine;

public partial class Creature : MonoBehaviour
{
    private void UpdateVitalSigns()
    {
        // 飢餓處理
        float currentHungerDepletion = isSleeping ? hungerRate * 0.5f : hungerRate;
        hunger = Mathf.Clamp(hunger - currentHungerDepletion, 0, maxHunger);

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
                col.radius = 1.3f;


                //    // 取圖片寬高之中較大的一半作為半徑，確保能包覆
                //    float maxDim = Mathf.Max(loadedSprite.bounds.size.x, loadedSprite.bounds.size.y);
                //    col.radius = maxDim / 2f * 0.95f;
                //    col.offset = loadedSprite.bounds.center;
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

    private void AttributeInheritance(Species species, CreatureAttributes creatureAttributes, GameObject creature_object)
    {
        float variationFactor() => UnityEngine.Random.Range(-species.variation, species.variation);
        //睡眠時間變異
        int delta_sleep_time() => (int)((creatureAttributes.sleeping_tail - creatureAttributes.sleeping_head) * variationFactor());
        sleepingHead = creatureAttributes.sleeping_head + delta_sleep_time();
        sleepingTail = creatureAttributes.sleeping_tail + delta_sleep_time();
        //其他玩家屬性變異
        size = creatureAttributes.size + creatureAttributes.size * variationFactor();
        speed = creatureAttributes.speed + creatureAttributes.speed * variationFactor();
        maxHealth = creatureAttributes.max_health + creatureAttributes.max_health * variationFactor();
        reproductionRate = creatureAttributes.reproduction_rate + creatureAttributes.reproduction_rate * variationFactor();
        attackPower = creatureAttributes.attack_power + creatureAttributes.attack_power * variationFactor();
        lifespan = creatureAttributes.lifespan + creatureAttributes.lifespan * variationFactor();
        perceptionRange = creatureAttributes.perception_range + creatureAttributes.perception_range * variationFactor();
        //計算衍生屬性
        sleepTime = sleepingTail - sleepingHead;
        hungerRate = AttributesCalculator.CalculateHungerRate(size, speed, attackPower);
        maxHunger = AttributesCalculator.CalculateMaxHunger(size, maxHealth, foodTypes);
        healthRegeneration = AttributesCalculator.CalculateHealthRegeneration(maxHealth, size, sleepTime);
        //初始狀態
        hunger = maxHunger;
        health = maxHealth;
        age = 0;
        actionCooldown = 0;
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
}
