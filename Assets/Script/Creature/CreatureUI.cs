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
        // 生存與生理需求
        case ActionType.Eat:        targetColor = new Color(0.4f, 0.9f, 0.4f); break; // 明亮的草綠
        case ActionType.Sleep:      targetColor = new Color(0.6f, 0.8f, 1f); break;   // 柔和的淡藍
        case ActionType.Daze:       targetColor = Color.gray; break;                  // 發呆用灰色，降低存在感

        // 社交與行為模式
        case ActionType.Wander:     targetColor = Color.white; break;                 // 漫遊用白色，作為預設基底
        case ActionType.Flock:      targetColor = new Color(0.8f, 0.6f, 1f); break;   // 集群用淡紫色，有種連結感

        // 戰鬥相關 (暖色系，強調警示)
        case ActionType.Attack:     targetColor = new Color(1f, 0.3f, 0.3f); break;   // 純紅色，強調攻擊
        case ActionType.Retaliate:  targetColor = new Color(0.9f, 0.5f, 0.1f); break; // 橘色，反擊的衝擊感
        case ActionType.Flee:       targetColor = Color.yellow; break;                // 黃色，危險警告

        // 繁衍相關 (粉色系，區別於一般行為)
        case ActionType.Mating:     targetColor = new Color(1f, 0.6f, 0.8f); break;   // 較淺的粉，求偶階段
        case ActionType.Reproduce:  targetColor = new Color(1f, 0.4f, 0.7f); break;   // 較深的粉，產卵/完成階段

        default: targetColor = Color.white; break;
    }
    actionText.color = targetColor;
}
}