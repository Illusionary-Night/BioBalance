using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

public static class ActionSystem
{
    private static readonly Dictionary<ActionType, ActionBase> actions = new();

    static ActionSystem()
    {
        ActionSystem.Register();
    }

    public static void Register()
    {
        // 掃描所有繼承 ActionBase 的型別
        var actionTypes = typeof(ActionBase).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ActionBase)));

        foreach (var type in actionTypes)
        {
            // 強制建立該類別的 Instance 靜態欄位
            var instanceField = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
            if (instanceField?.GetValue(null) is ActionBase action)
                actions.Add(action.Type, action);
        }
    }

    public static bool IsConditionMet(Creature creature, ActionType actiontype)
    {
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



