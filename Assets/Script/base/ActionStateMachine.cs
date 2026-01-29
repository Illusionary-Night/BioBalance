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

    //給creature editor那邊監控用---
    public bool HasMovementCallback => currentMovementCallback != null;
    public string CurrentActionName => currentContext?.ActionType.ToString() ?? "None";
    
    // 用於 Creature Editor 監控的快取數據
    public struct ActionDebugInfo
    {
        public bool isConditionMet;
        public float weight;
    }
    public Dictionary<ActionType, ActionDebugInfo> DebugInfoCache = new();

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
        DebugInfoCache.Clear();

        // 收集可用的 Actions
        List<KeyValuePair<ActionType, float>> availableActions = new List<KeyValuePair<ActionType, float>>();

        for (int i = 0; i < owner.actionList.Count; i++)
        {
            // 加入存取一些給Creature Editor監測的數據
            ActionType type = owner.actionList[i];
            bool met = ActionSystem.IsConditionMet(owner, type);
            float weight = met ? ActionSystem.GetWeight(owner, type) : 0;

            // 存入快取供 Editor 讀取
            DebugInfoCache[type] = new ActionDebugInfo { isConditionMet = met, weight = weight };

            if (met)
            {
                availableActions.Add(new KeyValuePair<ActionType, float>(type, weight));
            }
        }

        // 按權重排序
        availableActions.Sort((x, y) => y.Value.CompareTo(x.Value));

        // 嘗試執行 Action
        while (availableActions.Count > 0)
        {
            ActionType selectedAction = availableActions[0].Key;
            
            // 如果有正在執行的 Action，先清理
            if (IsExecuting)
            {
                // 取消當前 Action
                CancelCurrentAction();
            }

            if (ActionSystem.IsSuccess(owner, selectedAction))
            {
                // 創建新的執行上下文
                currentContext = new ActionContext(owner, selectedAction);
                
                // 訂閱上下文事件
                currentContext.OnCancelled += OnActionCancelled;
                currentContext.OnCompleted += OnActionCompleted;
                
                // 執行 Action
                owner.SetCurrentAction(selectedAction);
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
            //int cooldown = ActionSystem.GetCooldown(owner, currentContext.ActionType);
            //owner.actionCooldown = cooldown;
            owner.ResetActionCooldown(currentContext.ActionType);
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