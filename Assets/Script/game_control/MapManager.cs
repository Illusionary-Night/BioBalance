using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Core")]
    [SerializeField] private MapPipeline pipeline;
    [SerializeField] private MapVisualizer visualizer;
    
    [Header("Default Steps")]
    [SerializeField] private HeightMapStep heightStep;
    [SerializeField] private TerrainDecisionStep decisionStep;
    [SerializeField] private TilemapRenderStep renderStep;
    
    // === 公開屬性 (相容舊版) ===
    public TerrainMap GetDefinitionMap() => pipeline.OutputMap;
    public int mapWidth => pipeline.Width;
    public int mapHeight => pipeline.Height;
    
    private void Awake()
    {
        SetupDefaultPipeline();
    }
    
    /// <summary>
    /// 設定預設 Pipeline (還原舊版效果)
    /// </summary>
    public void SetupDefaultPipeline()
    {
        pipeline.ClearSteps();
        
        // 基礎三步驟
        pipeline.AddStep(heightStep);
        pipeline.AddStep(decisionStep);
        pipeline.AddStep(renderStep);
        
        renderStep.Visualizer = visualizer;
    }
    
    /// <summary>
    /// 在指定階段插入自訂步驟
    /// </summary>
    public void InsertStep(IMapPipelineStep step)
    {
        pipeline.AddStep(step);
    }
    
    // === 操作方法 (相容 Editor) ===
    public void GenerateDataOnly()
    {
        renderStep.Enabled = false;
        pipeline.Execute();
        renderStep.Enabled = true;
    }
    
    public void DrawDebugLayer()
    {
        if (pipeline.OutputMap == null) return;
        visualizer.InitializeDebugResources();
        visualizer.RenderDebugMap(pipeline.OutputMap, pipeline.Width, pipeline.Height);
    }
    
    public void DrawDualGridLayer()
    {
        if (pipeline.OutputMap == null) return;
        visualizer.RenderDualGridMap(pipeline.OutputMap, pipeline.Width, pipeline.Height);
    }
    
    public void GenerateAndDrawAll()
    {
        SetupDefaultPipeline();
        pipeline.Execute();
    }
    
    public void RollAndRedraw()
    {
        pipeline.RandomizeSeed();
        GenerateAndDrawAll();
    }
    
    public void ResetMap()
    {
        visualizer?.ClearAll();
    }
}