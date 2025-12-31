using UnityEngine;

public class SelectionIndicator : IndicatorBase
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
        var dest = targetCreature.transform.position;
        transform.position = new Vector3(dest.x, dest.y, 0);
    }
    void CreateCircle(float radius, float lineWidth)
    {
        LineRenderer line = GetComponent<LineRenderer>();
        line.positionCount = 51; // ½u¬q²Ó½o«×
        line.widthMultiplier = lineWidth;
        line.useWorldSpace = false;

        float deltaTheta = (2f * Mathf.PI) / 50;
        float theta = 0f;

        for (int i = 0; i < 51; i++)
        {
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            line.SetPosition(i, new Vector3(x, y, 0));
            theta += deltaTheta;
        }
    }
}