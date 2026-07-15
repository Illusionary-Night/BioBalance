using UnityEngine;

public class AgeAttr : FloatAttribute
{
    public float maxAge { get; private set; }
    public float agingRate { get; private set; }
    public float Percentage => maxAge > 0 ? Mathf.Clamp01(CalculateFinalValue() / maxAge) : 0f;
    public AgeAttr(float maxValue, float increaseRate = 1, float multiplier = 1) : base(0, multiplier)
    {
        maxAge = maxValue > 0 ? maxValue : 0;
        agingRate = increaseRate > 0 ? increaseRate : 0;
    }

    protected override void Validate()
    {
        _baseValue = Mathf.Clamp(_baseValue, 0, maxAge);

        if (_multiplier < 0)
        {
            // LogManager.LogWarning("Age multiplier cannot be negative. Setting to 0.");
            _multiplier = 0;
        }
    }
    public void Aging()
    {
        Add(agingRate);
    }
}
