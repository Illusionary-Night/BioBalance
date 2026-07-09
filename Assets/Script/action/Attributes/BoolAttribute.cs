using UnityEngine;

public class BoolAttribute : IValueAttribute<bool>
{
    private bool _value;

    public bool Query()
    {
        return _value;
    }

    public bool Set(bool newValue)
    {
        _value = newValue;
        return true;
    }

    public BoolAttribute(bool initialValue)
    {
        _value = initialValue;
    }
}
