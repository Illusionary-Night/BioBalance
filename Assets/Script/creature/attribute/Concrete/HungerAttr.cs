using UnityEngine;

public class HungerAttr : FloatAttribute
{
    public float maxHunger { get; private set; }
    public float hungerRate { get; private set; }
    public float Percentage => maxHunger > 0 ? Mathf.Clamp01(CalculateFinalValue() / maxHunger) : 0f;
    public HungerAttr(float maxValue, float decreaseRate, float multiplier = 1) : base(maxValue, multiplier)
    {
        maxHunger = maxValue > 0 ? maxValue : 0;
        hungerRate = decreaseRate > 0 ? decreaseRate : 0;
    }

    public override bool Inherit(CreatureData father, CreatureData mother, CreatureData selfData)
    {
        HungerAttr fatherAttr = father.GetAttribute<HungerAttr>();
        HungerAttr motherAttr = mother.GetAttribute<HungerAttr>();
        maxHunger = CalculateMean(fatherAttr.maxHunger, motherAttr.maxHunger);
        hungerRate = CalculateMean(fatherAttr.hungerRate, motherAttr.hungerRate);
        SetBaseValue(maxHunger);
        return true;
    }

    protected override void Validate()
    {
        _baseValue = Mathf.Clamp(_baseValue, 0, maxHunger);

        if (_multiplier < 0)
        {
            // LogManager.LogWarning("Hunger multiplier cannot be negative. Setting to 0.");
            _multiplier = 0;
        }
    }
    public void Digest()
    {
        Add(-hungerRate);
    }
}
