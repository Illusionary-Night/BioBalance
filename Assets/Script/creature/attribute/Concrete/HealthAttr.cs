using UnityEngine;

public class HealthAttr : FloatAttribute
{
    public float maxHealth { get; private set; }
    public float regenerationRate { get; private set; }
    public float Percentage => maxHealth > 0 ? Mathf.Clamp01(CalculateFinalValue() / maxHealth) : 0f;
    public HealthAttr(float maxValue, float increaseRate, float multiplier = 1) : base(maxValue, multiplier)
    {
        maxHealth = maxValue > 0 ? maxValue : 0;
        regenerationRate = increaseRate > 0 ? increaseRate : 0;
    }

    protected override void Validate()
    {
        _baseValue = Mathf.Clamp(_baseValue, 0, maxHealth);

        if (_multiplier < 0)
        {
            // LogManager.LogWarning("Health multiplier cannot be negative. Setting to 0.");
            _multiplier = 0;
        }
    }
    public void Regenerate()
    {
        Add(regenerationRate);
    }
}
