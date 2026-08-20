using UnityEngine;

public class GenderAttr : EnumAttribute<Gender>
{
    public GenderAttr(Gender initialValue) : base(initialValue)
    {
    }

    public bool Inherit(CreatureData fatherData, CreatureData motherData, CreatureData selfData)
    {
        GenderAttr fatherAttr = fatherData.GetAttribute<GenderAttr>();
        GenderAttr motherAttr = motherData.GetAttribute<GenderAttr>();

        Gender gender = Random.Range(0, 2) == 0 ? fatherAttr.Query() : motherAttr.Query();

        Set(gender);
        return true;
    }
}