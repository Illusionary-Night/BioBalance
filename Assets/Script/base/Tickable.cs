using UnityEngine;
public interface ITickable
{
    // This method is called once per tick by the Manager.
    void OnTick();
    // This method is called when the object is enabled.
    // Must subscribe to the Manager's OnTick event here.
    void OnEnable();
    // This method is called when the object is disabled.
    // Must unsubscribe from the Manager's OnTick event here.
    void OnDisable();
}