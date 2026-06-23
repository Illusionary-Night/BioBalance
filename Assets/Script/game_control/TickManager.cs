using UnityEngine;
using System.Collections.Generic;
using System;
//TODO: 需要把各種行動步驟分在不同執行時間，以免效能尖峰
public class TickManager : MonoBehaviour
{
    public int CurrentHour { get; private set; }
    public int CurrentDay { get; private set; }

    private readonly List<Action> tickable = new();
    private int tickCount = 0;
    private int TicksPerSecond = 30;
    private float realtime_counter = 0;
    private bool isPaused = false;

    private void OnEnable()
    {
        RegisterPauseInput();
    }

    private void OnDisable()
    {
        UnregisterPauseInput();
    }

    private void RegisterPauseInput()
    {
        MainManager.inputManager.OnPausePerformed += OnPausePerformed;
    }

    private void UnregisterPauseInput()
    {
        MainManager.inputManager.OnPausePerformed -= OnPausePerformed;
    }

    private void OnPausePerformed()
    {
        SetPause();
    }

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

    public void SetTPS(int TPS)
    {
        if (TPS > 0) TicksPerSecond = TPS;
    }

    public void SetPause(bool? pause = null)
    {
        if (!pause.HasValue) isPaused = !isPaused;
        else isPaused = (bool)pause;
    }

    private void Tick()
    {
        tickCount++;
        int total_hours = tickCount / constantData.TICKS_PER_HOUR;
        CurrentHour = total_hours % constantData.HOURS_PER_DAY;
        CurrentDay = (total_hours / constantData.HOURS_PER_DAY) + 1;

        // Create a copy of the list to avoid modification during iteration
        var tickOnTime = new List<Action>(tickable);

        foreach (var t in tickOnTime)
        {
            t?.Invoke();
        }

        // Debug log every hour
        // if (tickCount % constantData.TICKS_PER_HOUR == 0) Debug.Log($"Now is {CurrentDay} days {CurrentHour} hours");
    }

    private void Update()
    {
        if (!isPaused) realtime_counter += Time.deltaTime;

        while (realtime_counter >= 1f / TicksPerSecond)
        {
            Tick();
            realtime_counter -= 1f / TicksPerSecond;
        }
    }
}
