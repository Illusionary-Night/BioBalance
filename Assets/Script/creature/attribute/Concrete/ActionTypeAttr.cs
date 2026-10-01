using UnityEngine;

public class ActionTypeAttr : EnumAttribute<ActionType>
{
    public ActionTypeAttr(ActionType initialValue) : base(initialValue)
    {
    }
}
