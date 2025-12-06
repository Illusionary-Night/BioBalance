using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Core component")]
    [Tooltip("負責計算數據")]
    [SerializeField] private TerrainGenerator generator;

    [Tooltip("負責繪製畫面")]
    [SerializeField] private MapVisualizer visualizer;

    // 按鈕功能:只生成數據
    public void GenerateDataOnly()
    {
        if (EnsureComponents())
        {
            generator.GenerateMapData();
        }
    }

    // 按鈕功能:繪製 Debug 層
    public void DrawDebugLayer()
    {
        if (EnsureComponents() && CheckData())
        {
            if (!Application.isPlaying) visualizer.InitializeDebugResources();
            visualizer.RenderDebugMap(generator.GetDefinitionMap(), generator.mapWidth, generator.mapHeight);
        }
    }

    // 按鈕功能:繪製 Dual-Grid 層
    public void DrawDualGridLayer()
    {
        if (EnsureComponents() && CheckData())
        {
            visualizer.RenderDualGridMap(generator.GetDefinitionMap(), generator.mapWidth, generator.mapHeight);
        }
    }

    // 按鈕功能:全部重來
    public void GenerateAndDrawAll()
    {
        GenerateDataOnly();
        DrawDebugLayer();
        DrawDualGridLayer();
    }
    // 按鈕功能:隨機換種子並重畫(Roll & Redraw)
    public void RollAndRedraw()
    {
        if (EnsureComponents())
        {
            // 1. 換種子
            generator.RandomizeOffset();

            // 2. 生成數據 (用新種子算)
            generator.GenerateMapData();

            // 3. 繪製所有圖層 (Debug + Dual-Grid)
            // (您可以決定要不要重畫 Debug 層，通常為了效能可以只畫 Dual-Grid，或者全部都畫)
            DrawDebugLayer();
            DrawDualGridLayer();

            Debug.Log("MapManager: 已隨機重置並重新繪製地圖。");
        }
    }

    public void ResetMap()
    {
        if (generator != null) generator.ClearData();
        if (visualizer != null) visualizer.ClearAll();
        Debug.Log("MapManager: 地圖已重置 (Reset)。");
    }

    // --- 輔助檢查 ---
    private bool EnsureComponents()
    {
        if (generator == null || visualizer == null)
        {
            Debug.LogError("MapManager: 請在 Inspector 中指定 Generator 和 Visualizer！");
            return false;
        }
        return true;
    }

    private bool CheckData()
    {
        if (generator.GetDefinitionMap() == null)
        {
            Debug.LogWarning("MapManager: 沒有地圖數據！請先執行 'Generate Data Only'。");
            return false;
        }
        return true;
    }
}