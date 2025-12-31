using UnityEngine;

public class DestinationIndicator : IndicatorBase
{
    Creature targetCreature;
    public void SetTarget(Creature creature)
    {
        targetCreature = creature;
    }
    public override void UpdateIndicator()
    {
        if (targetCreature == null)
        {
            Debug.Log("targetCreature is null");
            Hide();
        }
        var dest = targetCreature.GetMovementDestination();
        transform.position = new Vector3(dest.x, dest.y, 0);
    }
}