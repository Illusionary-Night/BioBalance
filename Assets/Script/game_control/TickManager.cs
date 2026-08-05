using UnityEngine;
using System.Collections.Generic;
using System;

public class TickManager : MonoBehaviour
{
    public int CurrentHour { get; private set; }
    public int CurrentDay { get; private set; }

    [Tooltip("In Game base TPS")]
    public int MainTPS = 30;

    private int mainTickCount = 0;
    private float mainRealtimeCounter = 0;
    private bool isPaused = false;

    private class TickGroup
    {
        public int tps;
        public float counter;
        public List<Action> actions = new List<Action>();
    }

    private Dictionary<int, TickGroup> tickGroups = new Dictionary<int, TickGroup>();

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
        if (MainManager.inputManager != null)
            MainManager.inputManager.OnPausePerformed += OnPausePerformed;
    }

    private void UnregisterPauseInput()
    {
        if (MainManager.inputManager != null)
            MainManager.inputManager.OnPausePerformed -= OnPausePerformed;
    }

    private void OnPausePerformed()
    {
        SetPause();
    }

    /// <summary>
    /// Register Tick Event (targetTPS)
    /// </summary>
    public void RegisterTickable(Action onTick, int targetTPS = 30)
    {
        if (targetTPS <= 0) return;

        // 如果該 TPS 群組還不存在，就建立一個新的
        if (!tickGroups.TryGetValue(targetTPS, out TickGroup group))
        {
            group = new TickGroup { tps = targetTPS, counter = 0 };
            tickGroups[targetTPS] = group;
        }

        if (!group.actions.Contains(onTick))
        {
            group.actions.Add(onTick);
        }
    }

    /// <summary>
    /// Unregister Tick Event, requires the target TPS used during registration
    /// </summary>
    public void UnregisterTickable(Action onTick, int targetTPS = 30)
    {
        if (tickGroups.TryGetValue(targetTPS, out TickGroup group))
        {
            if (group.actions.Contains(onTick))
            {
                group.actions.Remove(onTick);
            }
        }
    }

    public void SetPause(bool? pause = null)
    {
        if (!pause.HasValue) isPaused = !isPaused;
        else isPaused = pause.Value;
    }

    // 獨立處理遊戲世界的時間推演
    private void UpdateGameTime()
    {
        mainTickCount++;
        int total_hours = mainTickCount / constantData.TICKS_PER_HOUR;
        CurrentHour = total_hours % constantData.HOURS_PER_DAY;
        CurrentDay = (total_hours / constantData.HOURS_PER_DAY) + 1;
    }

    private void Update()
    {
        if (isPaused) return;

        float dt = Time.deltaTime;

        // Main game timer
        mainRealtimeCounter += dt;
        float mainInterval = 1f / MainTPS;
        while (mainRealtimeCounter >= mainInterval)
        {
            UpdateGameTime();
            mainRealtimeCounter -= mainInterval;
        }

        foreach (var kvp in tickGroups)
        {
            TickGroup group = kvp.Value;
            group.counter += dt;
            float interval = 1f / group.tps;

            while (group.counter >= interval)
            {
                // Create a copy of the list to avoid modification during iteration
                var tickOnTime = new List<Action>(group.actions);

                foreach (var t in tickOnTime)
                {
                    t?.Invoke();
                }

                group.counter -= interval;
            }
        }
    }
}