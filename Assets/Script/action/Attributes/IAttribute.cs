using UnityEngine;

// Base interface for all attribute (Marker Interface)
public interface IAttribute {
    bool Inherit(CreatureData fatherData, CreatureData motherData, CreatureData selfData) { return true; }
}
