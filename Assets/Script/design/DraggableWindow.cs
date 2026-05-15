using UnityEngine;
using UnityEngine.EventSystems; // 必須引入這個才能接收 UI 事件

// 繼承 IDragHandler (拖曳中) 與 IBeginDragHandler (開始拖曳)
public class DraggableWindow : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;

    private void Awake()
    {
        // 抓取自己的 UI 變形組件
        rectTransform = GetComponent<RectTransform>();

        // 往上層找，找到最頂層的 Canvas
        // (這非常重要！因為 Canvas 縮放會影響滑鼠拖曳的距離計算)
        canvas = GetComponentInParent<Canvas>();
    }

    // 當滑鼠「點下去並開始移動」的瞬間觸發
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Vibe 體驗優化：當你點擊拖曳某個視窗時，它應該要跑到最上層，蓋住其他東西
        transform.SetAsLastSibling();
    }

    // 當滑鼠「拖曳中」每幀觸發
    public void OnDrag(PointerEventData eventData)
    {
        // 核心邏輯：將視窗的位置，加上滑鼠的移動量 (eventData.delta)
        // ⚠️ 必須除以 canvas.scaleFactor，否則如果你的螢幕有縮放，視窗移動速度會跟滑鼠對不上！
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}