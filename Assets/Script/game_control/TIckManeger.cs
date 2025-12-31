using UnityEngine;
using System.Collections.Generic;
using System;

public class TickManeger: MonoBehaviour
{
    public static TickManeger Instance { get; private set; }

    public TickManeger()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("TickManeger instance already exists!");
            return;
        }
        Instance = this;
    }

    private readonly List<Action> tickable = new(); 
    private int tickCount = 0;
    private const int TicksPerSecond = 30;
    private float realtime_counter = 0;

    public void RegisterTickable(Action onTick)
    {
        if (!this.tickable.Contains(onTick))
        {
            this.tickable.Add(onTick);
        }
    }

    public void UnregisterTickable(Action onTick)
    {
        if (this.tickable.Contains(onTick))
        {
            this.tickable.Remove(onTick);
        }
    }

    private void Tick()
    {
        tickCount++;
        foreach (var t in tickable)
        {
            t?.Invoke();
        }
    }

    private void Update()
    {
        realtime_counter += Time.deltaTime;

        while (realtime_counter >= 1f / TicksPerSecond)
        {
            Tick();
            realtime_counter -= 1f / TicksPerSecond;
        }
    }
}
