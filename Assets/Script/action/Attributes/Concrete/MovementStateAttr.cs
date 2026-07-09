using UnityEngine;

public class MovementStateAttr : EnumAttribute<CreatureMovementState>
{
    public MovementStateAttr(CreatureMovementState initialValue) : base(initialValue)
    {
    }
}
