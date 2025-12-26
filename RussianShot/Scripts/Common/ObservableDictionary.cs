using System.Collections.Generic;
using System;
public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>
{
    public event Action OnChanged;

    public new TValue this[TKey key]
    {
        get => base[key];
        set
        {
            base[key] = value;
            OnChanged?.Invoke();
        }
    }

    public new void Add(TKey key, TValue value)
    {
        base.Add(key, value);
        OnChanged?.Invoke();
    }

    public new bool Remove(TKey key)
    {
        bool removed = base.Remove(key);
        if (removed) OnChanged?.Invoke();
        return removed;
    }
}
