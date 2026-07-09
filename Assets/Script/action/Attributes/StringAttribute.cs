using UnityEngine;

public class StringAttribute : IValueAttribute<string>
{
    private string _value;

    public string Query()
    {
        return _value;
    }

    public bool Set(string newValue)
    {
        _value = newValue;
        return true;
    }

    public StringAttribute(string initialValue)
    {
        _value = initialValue;
    }
}
