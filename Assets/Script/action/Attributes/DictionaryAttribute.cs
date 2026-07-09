using System.Collections.Generic;
using UnityEngine;

public class DictionaryAttribute<TKey, TValue> : IValueAttribute<IReadOnlyDictionary<TKey, TValue>>
{
    private readonly Dictionary<TKey, TValue> _value;

    public IReadOnlyDictionary<TKey, TValue> Query()
    {
        return _value;
    }

    public DictionaryAttribute(int initialCapacity = 0)
    {
        _value = new Dictionary<TKey, TValue>(initialCapacity);
    }

    public DictionaryAttribute(Dictionary<TKey, TValue> initialValue)
    {
        _value = new Dictionary<TKey, TValue>(initialValue);
    }

    // Encapsulated method to safely add items without duplicates
    public virtual bool AddItem(TKey key, TValue value)
    {
        if (!_value.ContainsKey(key))
        {
            _value.Add(key, value);
            OnItemAdded(key, value);
            return true;
        }
        return false; // Item already exists
    }

    // Encapsulated method to safely remove items
    public virtual bool RemoveItem(TKey key)
    {
        if (_value.Remove(key))
        {
            OnItemRemoved(key);
            return true;
        }
        return false; // Item not found
    }

    // Encapsulated method to query state
    public virtual bool HasItem(TKey key)
    {
        return _value.ContainsKey(key);
    }

    public virtual void Clear()
    {
        _value.Clear();
    }

    // Lifecycle hooks for subclasses to override (e.g., triggering events or validations)
    protected virtual void OnItemAdded(TKey key, TValue value) { }
    protected virtual void OnItemRemoved(TKey key) { }
}
