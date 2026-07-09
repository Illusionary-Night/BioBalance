using UnityEngine;

public class ColorAttr : IAttribute
{
    // TODO: 這邊不知道要有什麼檢查，所以先不做任何檢查，而且都給 public，以後記得改

    // 儲存六色比例，和必須為 1
    // 索引：0:紅, 1:橙, 2:黃, 3:綠, 4:藍, 5:紫
    public float[] colorGenes { get; set; } = new float[4];

    // 狀態判定
    public bool isWandering { get; set; } = false;
    public float fadeSpeed { get; set; } = 0.1f; // 褪色速度
    public bool isUsingColorGenes { get; set; } = true; // 是否啟用顏色基因影響外觀
    // 渲染相關
    public Renderer myRenderer { get; set; }
    public MaterialPropertyBlock propBlock { get; set; }

    [Header("Wandering Detection")]
    public float checkInterval { get; set; } = 1.0f;     // 判斷頻率：每 1 秒判斷一次即可
    public float _checkTimer { get; set; } = 0f;

    public float detectionRadius { get; set; } = 15.0f;   // 感應範圍
    public int minFamilyNeighbors { get; set; } = 1;     // 至少需要幾個「相似」同伴才不算走散
    public float similarityThreshold { get; set; } = 0.8f; // 基因差異容忍度 (數值越小，判斷越嚴格)

    // 強烈建議：將所有生物放在同一個 Layer (例如 "Creature")
    // 這樣 Physics2D 就不會去掃描地形或其他無關的物件
    public LayerMask creatureLayer { get; set; }
}
