using UnityEngine;

public abstract class FloatAttribute : MathAttribute<float>
{
    protected FloatAttribute(float initialValue, float multiplier = 1) : base(initialValue, multiplier)
    {
    }

    protected override float PerformAdd(float a, float b) => a + b;
    protected override float PerformSubtract(float a, float b) => a - b;
    protected override float PerformMultiply(float a, float b) => a * b;
    protected override float PerformDivide(float a, float b)
    {
        if (Mathf.Approximately(b, 0f))
        {
            LogManager.LogWarning("Attempted to divide by zero. Returning the original value.");
            return a; // Return the original value if division by zero is attempted
        }
        return a / b;
    }

    protected override float CalculateMean(float fatherValue, float motherValue)
    {
        return (fatherValue + motherValue) / 2.0f;
    }

    protected override float CalculateFinalValue()
    {
        return _baseValue * _multiplier;
    }
}
