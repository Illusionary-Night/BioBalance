using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public abstract class IndicatorBase : MonoBehaviour
{
    public abstract void UpdateIndicator();
    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
    protected virtual void Awake()
    {
        Hide();
    }
}