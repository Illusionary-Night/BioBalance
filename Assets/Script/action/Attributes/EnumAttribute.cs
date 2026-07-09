using UnityEngine;

public class EnumAttribute<TItem> : IValueAttribute<TItem> where TItem : System.Enum
{
    private TItem _value;

    public TItem Query()
    { 
        return _value; 
    }

    public bool Set(TItem newValue)
    {
        _value = newValue;
        return true;
    }

    public EnumAttribute(TItem initialValue)
    {
        _value = initialValue;
    }
}
