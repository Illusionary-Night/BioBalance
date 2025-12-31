using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;
using System;

public static class ActionSystem
{
    private static readonly Dictionary<ActionType, ActionBase> actions = new Dictionary<ActionType, ActionBase>();

    static ActionSystem()
    {
        ActionSystem.Register();
    }

    public static void Register()
    {
        var actionTypes = typeof(ActionBase).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(ActionBase)));

        foreach (var type in actionTypes)
        {
            try
            {
                var instanceField = type.GetField("Instance", BindingFlags.Public | BindingFlags.Static);
                ActionBase action = null;

                if (instanceField?.GetValue(null) is ActionBase staticAction)
                {
                    action = staticAction;
                }
                else
                {
                    action = (ActionBase)Activator.CreateInstance(type);
                }

                if (action != null)
                {
                    actions[action.Type] = action;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to register action {type.Name}: {ex.Message}");
            }
        }

        //Debug.Log($"Total registered actions: {actions.Count}");
    }

    public static bool IsConditionMet(Creature creature, ActionType actiontype)
    {
        return actions.TryGetValue(actiontype, out var f)
               && f.IsConditionMet(creature)
               && creature.GetActionCooldown(actiontype) <= 0;
    }

    public static float GetWeight(Creature creature, ActionType actiontype)
    {
        return actions.TryGetValue(actiontype, out var f) ? f.GetWeight(creature) : 0f;
    }

    public static int GetCooldown(Creature creature, ActionType actiontype)
    {
        return creature?.GetActionCooldown(actiontype) ?? 0;
    }

    public static bool IsSuccess(Creature creature, ActionType actiontype)
    {
        return actions.TryGetValue(actiontype, out var f) && f.IsSuccess(creature);
    }

    public static void Execute(Creature creature, ActionType actiontype, ActionContext context = null)
    {
        if (actions.TryGetValue(actiontype, out var f))
        {
            f.Execute(creature, context);
            creature.ResetActionCooldown(actiontype);
        }
    }
}

public abstract class ActionBase
{
    protected ActionBase() { }

    public abstract ActionType Type { get; }
    //public abstract int Cooldown { get; }

    public abstract bool IsConditionMet(Creature creature);
    public abstract float GetWeight(Creature creature);
    public abstract bool IsSuccess(Creature creature);
    
    // 新增 context 參數
    public abstract void Execute(Creature creature, ActionContext context = null);
}