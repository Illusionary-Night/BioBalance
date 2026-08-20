using UnityEngine;

public class AttackPowerAttr : FloatAttribute

{
    public AttackPowerAttr(float initialValue, float multiplier = 1) : base(initialValue, multiplier)
    {
    }
    protected override void Validate()
    {
        if (_baseValue < 0)
        {
            LogManager.LogWarning("Attack value cannot be negative. Setting to 0.");
            _baseValue = 0;
        }

        if (_multiplier < 0)
        {
            LogManager.LogWarning("Attack multiplier cannot be negative. Setting to 0.");
            _multiplier = 0;
        }
    }
}
