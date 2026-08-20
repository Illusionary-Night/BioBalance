using UnityEngine;

public abstract class MathAttribute<T> : IAttribute where T : struct
{
    protected T _baseValue;
    protected float _multiplier;

    public MathAttribute(T initialValue, float multiplier = 1.0f)
    {
        _baseValue = initialValue;
        _multiplier = multiplier;
        Validate();
    }

    // Public property to get the final calculated value
    public T Value => CalculateFinalValue();
    public float Multiplier => _multiplier;

    public virtual bool Inherit(CreatureData fatherData, CreatureData motherData, CreatureData selfData)
    {
        if (fatherData == null || motherData == null)
        {
            return false;
        }

        MathAttribute<T> fatherAttribute = fatherData.GetAttribute(GetType()) as MathAttribute<T>;
        MathAttribute<T> motherAttribute = motherData.GetAttribute(GetType()) as MathAttribute<T>;

        if (fatherAttribute == null || motherAttribute == null)
        {
            return false;
        }

        SetBaseValue(CalculateMean(fatherAttribute.Value, motherAttribute.Value));
        return true;
    }

    public void SetBaseValue(T value)
    {
        _baseValue = value;
        Validate();
    }

    public void Add(T amount)
    {
        _baseValue = PerformAdd(_baseValue, amount);
        Validate();
    }

    public void Subtract(T amount)
    {
        _baseValue = PerformSubtract(_baseValue, amount);
        Validate();
    }

    public void Multiply(T amount)
    {
        _baseValue = PerformMultiply(_baseValue, amount);
        Validate();
    }

    public void Divide(T amount)
    {
        _baseValue = PerformDivide(_baseValue, amount);
        Validate();
    }

    public void AddMultiplier(float additionalMultiplier)
    {
        _multiplier += additionalMultiplier;
        Validate();
    }

    public void ResetMultiplier()
    {
        _multiplier = 1.0f;
        Validate();
    }

    // --- Abstract Methods (To be implemented by concrete types) ---
    protected abstract T PerformAdd(T a, T b);
    protected abstract T PerformSubtract(T a, T b);
    protected abstract T PerformMultiply(T a, T b);
    protected abstract T PerformDivide(T a, T b);

    protected abstract T CalculateMean(T fatherValue, T motherValue);


    // Concrete class decides how to apply the float multiplier to type T
    protected abstract T CalculateFinalValue();

    // Optional validation hook (e.g., clamping values)
    protected virtual void Validate() { }
}
