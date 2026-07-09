using UnityEngine;

public abstract class IntAttribute : MathAttribute<int>
{
    protected IntAttribute(int initialValue, float multiplier = 1) : base(initialValue, multiplier)
    {
    }

    protected override int PerformAdd(int a, int b) => a + b;
    protected override int PerformSubtract(int a, int b) => a - b;
    protected override int PerformMultiply(int a, int b) => a * b;
    protected override int PerformDivide(int a, int b)
    {
        if (b == 0)
        {
            LogManager.LogWarning("Attempted to divide by zero. Returning the original value.");
            return a; // Return the original value if division by zero is attempted
        }
        return a / b;
    }
    protected override int CalculateFinalValue()
    {
        // Handle double to int conversion safely (e.g., Mathf.RoundToInt)
        return Mathf.RoundToInt(_baseValue * _multiplier);
    }
}
