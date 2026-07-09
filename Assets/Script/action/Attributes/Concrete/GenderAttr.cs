using UnityEngine;

public class GenderAttr : EnumAttribute<Gender>
{
    public GenderAttr(Gender initialValue) : base(initialValue)
    {
    }
}
