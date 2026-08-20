using UnityEngine;
using System.Collections.Generic;
using System;
public static class CreatureBuilder
{
    public static Creature Generate(Species species, CreatureData parentData1 = null, CreatureData parentData2 = null)
    {
        //TODO: 獲取creature，修改creature Pool
        Creature creature = new Creature();
        creature.data.species = species;
        Step1(creature.data, species, parentData1, parentData2);
        Step2(creature.data, species, parentData1, parentData2);

        return creature;
    }
    private static void Step1(CreatureData data, Species species, CreatureData parentData1, CreatureData parentData2)
    {
        data.size = Inherit(species, species.baseSize, parentData1?.size, parentData2?.size);
        data.speed = Inherit(species, species.baseSpeed, parentData1?.speed, parentData2?.speed);
        data.reproductionRate = Inherit(species, species.baseReproductionRate, parentData1?.reproductionRate, parentData2?.reproductionRate);
        data.perceptionRange = Inherit(species, species.basePerceptionRange, parentData1?.perceptionRange, parentData2?.perceptionRange);

        // 血量處理
        float _maxHealth = Inherit(species, species.baseMaxHealth, parentData1?.health?.maxHealth, parentData2?.health?.maxHealth);
        float _regenerationRata = AttributesCalculator.CalculateHealthRegeneration(_maxHealth, data.size);
        data.health = new HealthAttr(_maxHealth, _regenerationRata);

        // 飢餓處理
        float _maxHunger = AttributesCalculator.CalculateMaxHunger(data.size, _maxHealth, data.foodTypes);
        // 有magic number是因為先暫時解決報錯，之後會直接去修改計算公式
        float _hungerRate = AttributesCalculator.CalculateHungerRate(data.size, data.speed, 10);
        data.hunger = new HungerAttr(_maxHunger, _hungerRate);

        //年齡處理
        float _maxAge = Inherit(species, species.baseLifespan, parentData1?.age?.maxAge, parentData2?.age?.maxAge);
        float _agingRate = 1;
        data.age = new AgeAttr(_maxAge, _agingRate);

        return;

    }
    private static void Step2(CreatureData data, Species species, CreatureData parent1, CreatureData parent2)
    {
        if (species.actionList == null) return;

        HashSet<Type> requiredAttrTypes = new HashSet<Type>();

        foreach (ActionType actionType in species.actionList)
        {
            List<Type> types = ActionSystem.GetAttributeTypes(actionType);
            if (types != null)
            {
                foreach (Type type in types)
                {
                    requiredAttrTypes.Add(type);
                }
            }
        }

        foreach (Type attrType in requiredAttrTypes)
        {
            // 嘗試從父母身上尋找對應的基因
            IAttribute p1Attr = parent1?.GetAttribute(attrType);
            IAttribute p2Attr = parent2?.GetAttribute(attrType);

            // 實例化一個全新的技能屬性給孩子（避免共用記憶體）
            if (Activator.CreateInstance(attrType) is IAttribute newAttr)
            {
                if (p1Attr != null || p2Attr != null)
                {
                    // 【有遺傳來源】：父母有這個基因！
                    // TODO: 可以在這裡設計一個機制，把父母的數值 Copy 給 newAttr。
                    // 例如，你可以讓 IAttribute 實作一個介面 IInheritable，
                    // 然後呼叫：(newAttr as IInheritable)?.InheritFrom(p1Attr, p2Attr);
                }
                else
                {
                    // 【初代野生】：父母剛好都沒有這個基因，或者牠是孤兒
                    // 直接保持 newAttr 剛實例化出來的基礎數值即可
                }

                // 裝進背包
                data.AddAttribute(newAttr);
            }
        }
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
    public static float Inherit(Species species, float speciesVal, float? parent1Val, float? parent2Val)
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
