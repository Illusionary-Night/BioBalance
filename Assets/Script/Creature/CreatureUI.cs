using UnityEngine;
using TMPro;

public class CreatureUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionText;
    private Creature _owner;

    void Awake()
    {
        // 取得父物件上的 Creature 組件
        _owner = GetComponentInParent<Creature>();
    }

    void LateUpdate()
    {
        if (_owner == null) return;

        // 1. 位置修正：強制鎖定在生物的世界座標「正上方」
        // 無論生物本體怎麼旋轉，UI 的中心點永遠在 (生物位置 + 世界座標的向上位移)
        // 基礎高度 0.5f (保證至少在身體上方) + 隨體型變大的位移
        float verticalOffset = 1.0f + (_owner.size * 1.2f);
        transform.position = _owner.transform.position + Vector3.up * verticalOffset;

        // 2. 旋轉修正：看板效果 (Billboard)
        // 讓文字平面永遠面對相機
        transform.rotation = Camera.main.transform.rotation;

        // 3. 縮放修正：抗縮放 (保持 UI 大小一致)
        float pScale = _owner.transform.localScale.x;
        if (pScale > 0)
        {
            transform.localScale = Vector3.one * (0.05f / pScale);
        }

        actionText.text = _owner.currentAction.ToString();

        UpdateTextColor();
    }

    private void UpdateTextColor()
    {
        Color targetColor;
        switch (_owner.currentAction)
        {
            case ActionType.Flee: targetColor = Color.yellow; break;
            case ActionType.Attack: targetColor = new Color(1f, 0.45f, 0f); break; // 橘紅
            case ActionType.Eat: targetColor = Color.green; break;
            case ActionType.Sleep: targetColor = Color.cyan; break;
            case ActionType.Reproduce: targetColor = new Color(1f, 0.4f, 0.7f); break; // 粉色
            case ActionType.Wander: targetColor = Color.lightSkyBlue; break;
            default: targetColor = Color.white; break;
        }
        actionText.color = targetColor;
    }
}