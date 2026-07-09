using UnityEngine;
using System.Collections.Generic;

public abstract class ListAttribute<TItem> : IValueAttribute<IReadOnlyList<TItem>>
{
    private readonly List<TItem> _items;

    public IReadOnlyList<TItem> Query()
    {
        return _items;
    }

    public ListAttribute(int initialCapacity = 0)
    {
        _items = new List<TItem>(initialCapacity);
    }
    public ListAttribute(List<TItem> initialValue)
    {
        _items = new List<TItem>(initialValue);
    }

    // Encapsulated method to safely add items without duplicates
    public virtual bool AddItem(TItem item)
    {
        if (!_items.Contains(item))
        {
            _items.Add(item);
            OnItemAdded(item);
            return true;
        }
        return false; // Item already exists
    }

    // Encapsulated method to safely remove items
    public virtual bool RemoveItem(TItem item)
    {
        if (_items.Remove(item))
        {
            OnItemRemoved(item);
            return true;
        }
        return false; // Item not found
    }

    // Encapsulated method to query state
    public virtual bool HasItem(TItem item)
    {
        return _items.Contains(item);
    }

    public virtual void Clear()
    {
        _items.Clear();
    }

    // Lifecycle hooks for subclasses to override (e.g., triggering events or validations)
    protected virtual void OnItemAdded(TItem item) { }
    protected virtual void OnItemRemoved(TItem item) { }
}
