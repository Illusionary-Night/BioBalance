
using UnityEngine;

public class ReproductionCdAttr : FloatAttribute
{
    public int lastTime { get; private set; }
    public ReproductionCdAttr(float CDValue, float multiplier = 1) : base(CDValue, multiplier)
    {
        SetLastTime(MainManager.inGameManager.TickManager.tickCount);
    }
    public bool IsCooldownReady()
    {
        int current = MainManager.inGameManager.TickManager.tickCount;
        int cooldownTicks = Mathf.CeilToInt(Value);
        return current > lastTime + cooldownTicks;
    }
    public void SetLastTime(int tick)
    {
        lastTime = tick > 0 ? tick : 0;
    }
}