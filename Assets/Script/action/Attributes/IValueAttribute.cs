using UnityEngine;

public interface IValueAttribute<T> : IAttribute
{
    T Query();
}
