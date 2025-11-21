using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;

public static class ActionSystem
{
    private static readonly Dictionary<ActionType, ActionBase> actions = new();

    static ActionSystem()
    {
        ActionSystem.Register();
        Debug.Log("actions.Count "+actions.Count);
    }

    public static void Register()
    {
        // 掃描所有繼承 ActionBase 的型別
        var actionTypes = typeof(ActionBase).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ActionBase)));

        foreach (var type in actionTypes)
        {
            try
            {
                // 首先嘗試尋找 Instance 靜態欄位
                var instanceField = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                ActionBase action = null;

                if (instanceField?.GetValue(null) is ActionBase staticAction)
                {
                    action = staticAction;
                }
                else
                {
                    // 如果沒有 Instance 欄位，動態創建實例
                    action = (ActionBase)Activator.CreateInstance(type);
                }

                if (action != null)
                {
                    actions[action.Type] = action;
                    Debug.Log($"Registered action: {type.Name} for ActionType.{action.Type}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to register action {type.Name}: {ex.Message}");
            }
        }

        Debug.Log($"Total registered actions: {actions.Count}");
    }

    public static bool IsConditionMet(Creature creature, ActionType actiontype)
    {

        Debug.Log("Condition " + (actions.TryGetValue(actiontype, out var g)&& g.IsConditionMet(creature)));
        return actions.TryGetValue(actiontype, out var f) && f.IsConditionMet(creature);
    }

    public static float GetWeight(Creature creature, ActionType actiontype)
    {
        return actions.TryGetValue(actiontype, out var f) ? f.GetWeight(creature) : 0f;
    }

    public static int GetCooldown(Creature creature, ActionType actiontype)
    {
        return actions.TryGetValue(actiontype, out var f) ? f.Cooldown : 0;
    }

    public static bool IsSuccess(Creature creature, ActionType actiontype)
    {
        return actions.TryGetValue(actiontype, out var f) && f.IsSuccess(creature);
    }

    public static void Execute(Creature creature, ActionType actiontype)
    {
        if (actions.TryGetValue(actiontype, out var f))
            f.Execute(creature);
    }

    // 偵錯方法：顯示所有已註冊的動作
    public static void DebugRegisteredActions()
    {
        Debug.Log("=== Registered Actions ===");
        foreach (var kvp in actions)
        {
            Debug.Log($"ActionType.{kvp.Key} -> {kvp.Value.GetType().Name}");
        }
    }
}

public abstract class ActionBase
{
    protected ActionBase() { }

    public abstract ActionType Type { get; }
    public abstract int Cooldown { get; }

    public abstract bool IsConditionMet(Creature creature);
    public abstract float GetWeight(Creature creature);
    public abstract bool IsSuccess(Creature creature);
    public abstract void Execute(Creature creature);
}



