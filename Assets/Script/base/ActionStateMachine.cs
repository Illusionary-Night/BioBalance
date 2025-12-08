using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Action 狀態機，管理 Creature 的 Action 執行流程
/// </summary>
public class ActionStateMachine
{
    private Creature owner;
    private ActionContext currentContext;
    private System.Action<Vector2Int> currentMovementCallback;

    // 用於追蹤哪些事件處理器需要在清理時移除
    private List<System.Delegate> registeredCallbacks = new();

    public ActionContext CurrentContext => currentContext;
    public bool IsExecuting => currentContext != null && currentContext.IsValid;

    public ActionStateMachine(Creature creature)
    {
        owner = creature;
    }

    /// <summary>
    /// 評估並執行最佳 Action
    /// </summary>
    public void EvaluateAndExecute()
    {
        

        // 收集可用的 Actions
        List<KeyValuePair<ActionType, float>> availableActions = new List<KeyValuePair<ActionType, float>>();

        for (int i = 0; i < owner.ActionList.Count; i++)
        {
            if (ActionSystem.IsConditionMet(owner, owner.ActionList[i]))
            {
                float weight = ActionSystem.GetWeight(owner, owner.ActionList[i]);
                availableActions.Add(new KeyValuePair<ActionType, float>(owner.ActionList[i], weight));
            }
        }

        // 按權重排序
        availableActions.Sort((x, y) => y.Value.CompareTo(x.Value));

        // 更新權重列表（用於偵錯）
        owner.WeightedActionList.Clear();
        foreach (var action in availableActions)
        {
            owner.WeightedActionList.Add(action.Key);
        }

        // 嘗試執行 Action
        while (availableActions.Count > 0)
        {
            ActionType selectedAction = availableActions[0].Key;
            
            // 如果有正在執行的 Action，先清理
            if (IsExecuting)
            {
                if (currentContext.ActionType == selectedAction)
                {
                    // 如果正在執行的 Action 與選中的相同，則不需要重新執行
                    return;
                }
                else
                {
                    // 取消當前 Action
                    CancelCurrentAction();
                }
            }

            if (ActionSystem.IsSuccess(owner, selectedAction))
            {
                // 創建新的執行上下文
                currentContext = new ActionContext(owner, selectedAction);
                
                // 訂閱上下文事件
                currentContext.OnCancelled += OnActionCancelled;
                currentContext.OnCompleted += OnActionCompleted;
                
                // 執行 Action
                owner.CurrentAction = selectedAction;
                ActionSystem.Execute(owner, selectedAction, currentContext);

                return;
            }
            else
            {
                // 失敗，嘗試次高權重
                availableActions.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// 取消當前正在執行的 Action
    /// </summary>
    public void CancelCurrentAction()
    {
        if (currentContext != null)
        {
            currentContext.Cancel();
            Cleanup();
        }
    }

    /// <summary>
    /// 註冊移動完成回調
    /// </summary>
    public void RegisterMovementCallback(System.Action<Vector2Int> callback)
    {
        // 清除舊的回調
        ClearMovementCallback();
        
        currentMovementCallback = callback;
        owner.OnMovementComplete += callback;
        registeredCallbacks.Add(callback);
    }

    /// <summary>
    /// 清除移動回調
    /// </summary>
    private void ClearMovementCallback()
    {
        if (currentMovementCallback != null)
        {
            owner.OnMovementComplete -= currentMovementCallback;
            currentMovementCallback = null;
        }
    }

    /// <summary>
    /// Action 被取消時的處理
    /// </summary>
    private void OnActionCancelled()
    {
        Cleanup();
    }

    /// <summary>
    /// Action 完成時的處理
    /// </summary>
    private void OnActionCompleted()
    {
        // 設定冷卻時間
        if (currentContext != null)
        {
            int cooldown = ActionSystem.GetCooldown(owner, currentContext.ActionType);
            owner.ActionCooldown = cooldown;
        }
        
        Cleanup();
    }

    /// <summary>
    /// 清理所有事件訂閱和回調
    /// </summary>
    private void Cleanup()
    {
        // 清除移動回調
        ClearMovementCallback();
        
        // 清除所有註冊的回調
        registeredCallbacks.Clear();
        
        // 取消訂閱上下文事件
        if (currentContext != null)
        {
            currentContext.OnCancelled -= OnActionCancelled;
            currentContext.OnCompleted -= OnActionCompleted;
            currentContext = null;
        }
    }
}