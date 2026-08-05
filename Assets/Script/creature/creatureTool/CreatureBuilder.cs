using UnityEngine;
//
public static class CreatureBuilder
{
    // ==========================================
    // 共用廠房設置 (Singleton)
    // ==========================================

    // 當前的組裝材料
    public static CreatureData _data;
    private static Species _species;

    // 私有建構子，不允許外面的人使用 new CreatureBuilder()
    private CreatureBuilder() { }

    // ==========================================
    // 流水線組裝步驟
    // ==========================================

    /// <summary>
    /// 步驟一：建立基底
    /// </summary>
    //TODO: 改成CreatureBase
    public static CreatureBuilder Begin(Species species)
    {
        _instance._species = species;

        // 給一個全新的白板資料
        _instance._data = new CreatureData();
        _instance._data.species = species;

        return _instance;
    }
    /// <summary>
    /// 步驟二：核心遺傳與變異
    /// </summary>
    public CreatureBuilder ApplyGenetics(CreatureData parent1 = null, CreatureData parent2 = null)
    {
        // 呼叫原本的遺傳公式，計算體質並直接寫入半成品
        _data.size = Inherit(_species, _species.baseSize, parent1?.size, parent2?.size);
        _data.speed = Inherit(_species, _species.baseSpeed, parent1?.speed, parent2?.speed);
        _data.reproductionRate = Inherit(_species, _species.baseReproductionRate, parent1?.reproductionRate, parent2?.reproductionRate);
        _data.perceptionRange = Inherit(_species, _species.basePerceptionRange, parent1?.perceptionRange, parent2?.perceptionRange);

        // 1. 血量
        float maxHP = Inherit(_species, _species.baseMaxHealth, parent1?.health?.maxHealth, parent2?.health?.maxHealth);
        float regenRate = AttributesCalculator.CalculateHealthRegeneration(maxHP, _data.size);
        _data.health = new HealthAttr(maxHP, regenRate);

        // 2. 飢餓
        float maxHunger = AttributesCalculator.CalculateMaxHunger(_data.size, maxHP, _species.foodTypes);
        float hungerRate = AttributesCalculator.CalculateHungerRate(_data.size, _data.speed, _species.baseAttackPower);
        _data.hunger = new HungerAttr(maxHunger, hungerRate);

        // 3. 年齡
        float _maxAge = Inherit(_species, _species.baseLifespan, parent1?.age?.maxAge, parent2?.age?.maxAge);
        float _agingRate = 1;
        _data.age = new AgeAttr(_maxAge, _agingRate);


        // 性別判定
        // TODO: 不過話說性別判定的部分可能還需要再跟隊友考慮一下
        if (parent1 == null && parent2 == null)
        {
            _data.gender = _species.reproductionType == ReproductionType.Asexual ? Gender.None : (Random.value > 0.5f ? Gender.Male : Gender.Female);
        }
        else if (parent1 == null || parent2 == null)
        {
            _data.gender = parent1?.gender ?? parent2?.gender ?? Gender.None;
        }
        else
        {
            _data.gender = Random.value > 0.5f ? Gender.Male : Gender.Female;
        }

        return this;
    }

    /// <summary>
    /// 步驟三：建立行為衍生屬性
    /// </summary>
    public CreatureBuilder Assemble()
    {
        // 取Enum 清單，動態把行為對應的attr掛進 Attribute 背包裡
        if (_species.actionList != null)
        {
            //TODO: foreach list of attrs => inherit 
            foreach (ActionType actionType in _species.actionList)
            {
            }
        }
        return this;
    }

    /// <summary>
    /// 步驟四：出廠交貨
    /// </summary>
    public CreatureData Build()
    {
        // 將組裝好的資料抽出，工作台空出準備迎接下一次 Begin
        return _data;
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
    private float Inherit(Species species, float speciesVal, float? parent1Val, float? parent2Val)
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