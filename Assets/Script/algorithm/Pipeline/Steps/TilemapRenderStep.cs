using UnityEngine;

/// <summary>
/// Tilemap 渲染 - 連接 MapVisualizer
/// </summary>
[System.Serializable]
public class TilemapRenderStep : IMapPipelineStep
{
    [SerializeField] private MapVisualizer visualizer;
    [SerializeField] private bool renderDebug = true;
    [SerializeField] private bool renderDualGrid = true;
    [SerializeField] private bool enabled = true;
    
    public string StepName => "Tilemap Render";
    public PipelinePhase Phase => PipelinePhase.Visualization;
    public int Priority => 0;
    public bool Enabled { get => enabled; set => enabled = value; }
    
    public MapVisualizer Visualizer { get => visualizer; set => visualizer = value; }
    public bool RenderDebug { get => renderDebug; set => renderDebug = value; }
    public bool RenderDualGrid { get => renderDualGrid; set => renderDualGrid = value; }
    
    public void Execute(MapPipelineContext ctx)
    {
        if (visualizer == null)
        {
            Debug.LogWarning("[TilemapRenderStep] MapVisualizer 未指定！");
            return;
        }
        
        if (renderDebug)
        {
            visualizer.InitializeDebugResources();
            visualizer.RenderDebugMap(ctx.OutputMap, ctx.Width, ctx.Height);
        }
        
        if (renderDualGrid)
        {
            visualizer.RenderDualGridMap(ctx.OutputMap, ctx.Width, ctx.Height);
        }
    }
}