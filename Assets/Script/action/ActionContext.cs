using UnityEngine;

/// <summary>
/// Action 執行的上下文，用於追蹤和控制 Action 的生命週期
/// </summary>
public class ActionContext
{
    private bool isCancelled = false;
    private bool isCompleted = false;
    private Creature owner;
    private ActionType actionType;

    public bool IsCancelled => isCancelled;
    public bool IsCompleted => isCompleted;
    public Creature Owner => owner;
    public ActionType ActionType => actionType;

    public ActionContext(Creature creature, ActionType type)
    {
        owner = creature;
        actionType = type;
    }

    /// <summary>
    /// 取消此 Action
    /// </summary>
    public void Cancel()
    {
        if (!isCompleted)
        {
            isCancelled = true;
            OnCancelled?.Invoke();
        }
    }

    /// <summary>
    /// 標記 Action 為已完成
    /// </summary>
    public void Complete()
    {
        if (!isCancelled)
        {
            isCompleted = true;
            OnCompleted?.Invoke();
        }
    }

    /// <summary>
    /// 檢查 Context 是否仍然有效（未取消且未完成）
    /// </summary>
    public bool IsValid => !isCancelled && !isCompleted;

    // 事件：當 Action 被取消時觸發
    public event System.Action OnCancelled;
    
    // 事件：當 Action 完成時觸發
    public event System.Action OnCompleted;
}