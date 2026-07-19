public class GeneticsSystem
{
    /// <summary>
    /// 生物遺傳與基因組裝工廠。
    /// 根據物種設定與雙親基因，產出一份全新的個體資料（CreatureData）。
    /// 自動判斷初代生成、單親無性生殖或雙親有性生殖，並計算變異與衍生生理數值。
    /// </summary>
    /// <param name="species">該生物所屬的物種（決定基礎數值與變異率）。</param>
    /// <param name="parentAttr1">雙親一號的基因資料。若為 null 且二號亦為 null，則視為初代野生個體。</param>
    /// <param name="parentAttr2">雙親二號的基因資料。若只有一號有值，則視為無性生殖或孤雌生殖。</param>
    /// <returns>回傳包含完成變異、性別決定以及衍生屬性推導的全新 CreatureData 實例。</returns>
    //TODO: 需要決定是否傳遞還要保留CreatureAttribute的類別，還是說統一使用CreatureData
    public static CreatureData AttributeInheritance(Species species, CreatureAttributes? parentAttr1 = null, CreatureAttributes? parentAttr2 = null)
    {
        CreatureData newCreatureData = new CreatureData();
        //TODO: 在賦予數值時包裝內會自動檢查邊界問題
        newCreatureData.size = Inherit(species, species.baseSize, parentAttr1?.size, parentAttr2?.size);
        newCreatureData.speed = Inherit(species, species.baseSpeed, parentAttr1?.speed, parentAttr2?.speed);
        newCreatureData.reproductionRate = Inherit(species, species.baseReproductionRate, parentAttr1?.reproduction_rate, parentAttr2?.reproduction_rate);
        newCreatureData.perceptionRange = Inherit(species, species.basePerceptionRange, parentAttr1?.perception_range, parentAttr2?.perception_range);

        // 血量處理
        float _maxHealth = Inherit(species, species.baseMaxHealth, parentAttr1?.max_health, parentAttr2?.max_health);
        float _regenerationRata = AttributesCalculator.CalculateHealthRegeneration(_maxHealth, newCreatureData.size);
        newCreatureData.health = new HealthAttr(_maxHealth, _regenerationRata);

        // 飢餓處理
        float _maxHunger = AttributesCalculator.CalculateMaxHunger(newCreatureData.size, _maxHealth, newCreatureData.foodTypes);
        float _hungerRate = AttributesCalculator.CalculateHungerRate(newCreatureData.size, newCreatureData.speed, newCreatureData.attackPower);
        newCreatureData.hunger = new HungerAttr(_maxHunger, _hungerRate);

        //年齡處理
        float _maxAge = Inherit(species, species.baseLifespan, parentAttr1?.lifespan, parentAttr2?.lifespan);
        float _agingRate = 1;
        newCreatureData.age = new AgeAttr(_maxAge, _agingRate);

        if (!parentAttr1.HasValue && !parentAttr2.HasValue)
        {
            //初代個體
            if (species.reproductionType == ReproductionType.Asexual)
            {
                newCreatureData.gender = Gender.None;
            }
            else
            {
                newCreatureData.gender = UnityEngine.Random.value > 0.5f ? Gender.Male : Gender.Female;
            }
        }
        else if (!parentAttr1.HasValue || !parentAttr2.HasValue)
        {
            // 【無性生殖/孤雌生殖】：直接拿單親的屬性
            newCreatureData.gender = parentAttr1?.gender ?? parentAttr2?.gender ?? Gender.None;
        }
        else
        {
            // 【有性生殖】：擲骰子決定拿爸爸還是媽媽的數值
            newCreatureData.gender = UnityEngine.Random.value > 0.5f ? Gender.Male : Gender.Female;
        }

        //計算衍生屬性
        //sleepTime = sleepingTail - sleepingHead;

        // 色彩基因初始化
        // float[] parent1Color = parentAttr1?.colorGenes;
        // float[] parent2Color = parentAttr2?.colorGenes;
        // InitializeColorGenes(parent1Color, parent2Color);

        return newCreatureData;

    }
    /// <summary>
    /// 核心數值遺傳演算法。
    /// 依序判定初代繼承、單親繼承或雙親隨機基因二選一，最後依照物種設定的變異係數進行隨機偏移。
    /// </summary>
    /// <param name="species">提供變異範圍設定（variation）的物種資料。</param>
    /// <param name="speciesVal">該物種預設的基礎數值（僅在初代生成時作為基準使用）。</param>
    /// <param name="parent1Val">親本一號的對應數值。</param>
    /// <param name="parent2Val">親本二號的對應數值。</param>
    /// <returns>經過變異公式疊加後的最終屬性數值。</returns>
    private static float Inherit(Species species, float speciesVal, float? parent1Val, float? parent2Val)
    {
        float baseValue = 0;
        if (!parent1Val.HasValue && !parent2Val.HasValue)
        {
            //初代個體
            baseValue = speciesVal;
        }
        else if (!parent2Val.HasValue)
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
        return baseValue + (baseValue * UnityEngine.Random.Range(-species.variation, species.variation));
    }
}
