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


    //TODO: 可能要考慮掛到Species上面
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
    //TODO: 有點忘記他為什麼要存在的東西
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

        // 色彩基因初始化
        float[] parent1Color = parentAttr1?.colorGenes;
        float[] parent2Color = parentAttr2?.colorGenes;
        InitializeColorGenes(parent1Color, parent2Color);

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

    public void InitializeColorGenes(float[] parent1Color, float[] parent2Color)
    {
        float mutationWeight = 0.1f;
        float inheritWeight = 1f - mutationWeight; // 0.9f

        // 1. 產生嚴格的 10% 隨機突變比例 (避免歸一化稀釋父母權重)
        float[] randomMutation = new float[6];
        float randomTotal = 0;
        for (int i = 0; i < 6; i++)
        {
            randomMutation[i] = UnityEngine.Random.value;
            randomTotal += randomMutation[i];
        }

        // 2. 依照 0、1、2 雙親狀態進行色彩分配
        for (int i = 0; i < 6; i++)
        {
            // 算出該顏色在此次突變中實際分到的比例 (加總保證等於 0.1)
            float strictRandomPart = (randomMutation[i] / randomTotal) * mutationWeight;

            float baseColorPart = 0f;

            if (parent1Color == null && parent2Color == null)
            {
                // 【初代個體】：0 個雙親，使用隨機色彩
                float[,] baseColor = new float[,] {
                    {1f, 0f, 0f, 0f, 0f, 0f},
                    {0f, 0f, 1f, 0f, 0f, 0f},
                    {0f, 0f, 0f, 0f, 1f, 0f}
                };
                baseColorPart = baseColor[UnityEngine.Random.Range(0, 3), i] * inheritWeight;
            }
            else if (parent2Color == null)
            {
                // 【無性生殖/孤雌生殖】：1 個雙親，繼承單親 90% 色彩
                baseColorPart = parent1Color[i] * inheritWeight;
            }
            else
            {
                // 【有性生殖】：2 個雙親，父母各佔 45% 色彩
                baseColorPart = (parent1Color[i] * (inheritWeight / 2f)) + (parent2Color[i] * (inheritWeight / 2f));
            }

            // 組合最終基因
            // 因為 inheritWeight (0.9) + mutationWeight (0.1) = 1.0
            // 所以組合出來的六色比例總和必定為 1，無需再次執行全陣列歸一化
            colorGenes[i] = baseColorPart + strictRandomPart;
        }
    }

    void UpdateColorGenes()
    {
        CheckWandering();
        if (isWandering)
        {
            float target = 1f / 6f; // 0.166... 平均值
            for (int i = 0; i < 6; i++)
            {
                colorGenes[i] = Mathf.Lerp(colorGenes[i], target, Time.deltaTime * fadeSpeed);
            }
            UpdateVisuals(); // 更新顏色展現
        }
    }
    private SpriteRenderer _spriteRenderer;

    // 定義六角形的基礎顏色 (這在 C# 裡定義)
    private Color[] _baseColors = new Color[] {
        new Color(1f, 0f, 0f),    // 紅
        new Color(1f, 0.5f, 0f),  // 橙
        new Color(1f, 1f, 0f),    // 黃
        new Color(0f, 1f, 0f),    // 綠
        new Color(0f, 0f, 1f),    // 藍
        new Color(0.5f, 0f, 1f)   // 紫
    };

    public void UpdateVisuals()
    {
        // 建立暫存陣列，保護原始基因資料 (總和維持 1)
        float[] renderColors = new float[6];
        System.Array.Copy(colorGenes, renderColors, 6);

        // 索引對應：0:紅, 1:橙, 2:黃, 3:綠, 4:藍, 5:紫

        // ==========================================
        // 步驟 1：分離律 (1 橘 -> 1 紅 + 1 黃)
        // ==========================================
        renderColors[0] += renderColors[1] + renderColors[5]; // 紅 += 橙 + 紫
        renderColors[2] += renderColors[1] + renderColors[3]; // 黃 += 橙 + 綠
        renderColors[4] += renderColors[3] + renderColors[5]; // 藍 += 綠 + 紫

        // 次要色概念已完全釋放至主要色，將其歸零
        renderColors[1] = 0f;
        renderColors[3] = 0f;
        renderColors[5] = 0f;

        // ==========================================
        // 步驟 2：抵銷律 (紅、黃、藍 1:1:1 相消)
        // ==========================================
        float minPrimary = Mathf.Min(renderColors[0], renderColors[2], renderColors[4]);
        renderColors[0] -= minPrimary;
        renderColors[2] -= minPrimary;
        renderColors[4] -= minPrimary;

        // 此時，紅、黃、藍必定至少有一者歸零。

        // ==========================================
        // 步驟 3：結合律 (1 紅 + 1 黃 -> 1 橙)
        // ==========================================
        if (renderColors[0] > 0 && renderColors[2] > 0)
        {
            // 剩下紅與黃 -> 聚合為橙色
            float combineAmt = Mathf.Min(renderColors[0], renderColors[2]);
            renderColors[1] += combineAmt;
            renderColors[0] -= combineAmt;
            renderColors[2] -= combineAmt;
        }
        else if (renderColors[2] > 0 && renderColors[4] > 0)
        {
            // 剩下黃與藍 -> 聚合為綠色
            float combineAmt = Mathf.Min(renderColors[2], renderColors[4]);
            renderColors[3] += combineAmt;
            renderColors[2] -= combineAmt;
            renderColors[4] -= combineAmt;
        }
        else if (renderColors[4] > 0 && renderColors[0] > 0)
        {
            // 剩下藍與紅 -> 聚合為紫色
            float combineAmt = Mathf.Min(renderColors[4], renderColors[0]);
            renderColors[5] += combineAmt;
            renderColors[4] -= combineAmt;
            renderColors[0] -= combineAmt;
        }

        // ==========================================
        // 最終輸出與歸一化 (確保顯示亮度正常)
        // ==========================================
        // 因為 1->2 與 2->1 的轉換會導致數值總和膨脹或收縮
        // 為了讓 SpriteRenderer 正確顯示，我們需要找出目前剩下的總能量並重新分配
        float remainingEnergy = 0f;
        for (int i = 0; i < 6; i++)
        {
            remainingEnergy += renderColors[i];
        }

        Color finalColor = Color.black;

        // 若完全抵銷 (例如原本剛好是紅黃藍各 1/3)，呈現無色/灰色
        if (remainingEnergy <= 0.0001f)
        {
            finalColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
        else
        {
            // 將剩餘的純化顏色依照比例轉為 RGB
            for (int i = 0; i < 6; i++)
            {
                renderColors[i] /= remainingEnergy; // 歸一化到 0~1 範圍
                finalColor += _baseColors[i] * renderColors[i];
            }
        }

        finalColor.a = 1f;
        _spriteRenderer.color = finalColor;
        // Debug.Log("Final Color: " + finalColor + " sprite: " + _spriteRenderer.sprite.name);
    }
    //TODO: Wander這個字要改掉，跟action裡面的一個東西重複了，會讓人誤會。
    private void CheckWandering()
    {
        // 1. 頻率限制 (Timer)
        _checkTimer -= Time.deltaTime;
        if (_checkTimer > 0f) return;

        // 重置計時器，加入一點隨機值避免所有生物在同一個 Frame 進行運算 (效能優化)
        _checkTimer = checkInterval + Random.Range(-0.1f, 0.1f);

        // 4. 更新走散狀態
        isWandering = !Perception.Creatures.HasTarget(this, speciesID, 0.5f);
    }
    private bool IsSimilarColor(Creature other)
    {
        float difference = 0f;

        // 將自己的六色與對方的六色逐一相減取絕對值
        for (int i = 0; i < 6; i++)
        {
            difference += Mathf.Abs(this.colorGenes[i] - other.colorGenes[i]);
        }

        // 說明：
        // 如果完全一樣，difference = 0
        // 如果完全極端 (例如我是 100% 紅，你是 100% 藍)，difference = 2
        // 如果 threshold 設為 0.4，代表允許兩者在六色分配上有 20% 的偏移
        return difference <= similarityThreshold;
    }
}

