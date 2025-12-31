using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IndicatorController : MonoBehaviour
{
    public static IndicatorController Instance { get; private set; }

    [SerializeField] private List<IndicatorBase> allIndicators; // 在 Inspector 把所有指示器元件拖進來
    private void Awake()
    {
        // 2. 建立 Instance (這是最關鍵的一步)
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("發現多個 IndicatorController，已刪除重複項。");
            Destroy(gameObject);
            return;
        }

        // 3. 自動抓取子物件中的指示器 (選用：這樣就不用在 Inspector 手動拖)
        if (allIndicators.Count == 0)
        {
            allIndicators.AddRange(GetComponentsInChildren<IndicatorBase>(true));
        }

        // 4. 確保一開始全部隱藏
        foreach (var ind in allIndicators) ind.Hide();
    }
    private void Update()
    {

        // 遍歷所有指示器，如果它是啟用的，就叫它更新
        foreach (var indicator in allIndicators)
        {
            if (indicator.gameObject.activeSelf)
            {
                indicator.UpdateIndicator();
            }
        }
    }
    public T GetIndicator<T>() where T : IndicatorBase
    {
        // 使用 LINQ 的 OfType<T> 篩選出符合型別的物件，並取第一個
        return allIndicators.OfType<T>().FirstOrDefault();
    }
}