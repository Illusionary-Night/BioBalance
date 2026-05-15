using UnityEngine;
using UnityEngine.UI;

public class DesignMenuManager : MonoBehaviour
{
    [Header("主控制")]
    public GameObject designCanvas; // 整個設計介面的畫布
    public GameObject popupContainer; // 彈出視窗的半透明黑底容器

    [Header("彈出視窗清單 (對應左側按鈕)")]
    // 0:基底, 1:屬性, 2:潛因, 3:行動頻率
    public GameObject[] popupPanels; 

    private void Start()
    {
        // 一開始先隱藏所有彈出視窗
        CloseAllPopups();
    }

    /// <summary>
    /// 綁定在左側 4 個按鈕上的 OnClick 事件
    /// </summary>
    /// <param name="index">傳入 0, 1, 2, 3</param>
    public void OpenPopup(int index)
    {
        // 1. 打開彈出視窗的容器底色
        popupContainer.SetActive(true);

        // 2. 關閉所有的視窗
        for (int i = 0; i < popupPanels.Length; i++)
        {
            popupPanels[i].SetActive(false);
        }

        // 3. 只打開被選中的那個視窗
        if (index >= 0 && index < popupPanels.Length)
        {
            popupPanels[index].SetActive(true);
        }
    }

    /// <summary>
    /// 點擊彈出視窗外圍的黑底，或是按下確認/打叉按鈕時呼叫
    /// </summary>
    public void CloseAllPopups()
    {
        popupContainer.SetActive(false);
        for (int i = 0; i < popupPanels.Length; i++)
        {
            popupPanels[i].SetActive(false);
        }
    }
}