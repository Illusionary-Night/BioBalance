using UnityEngine;

/// <summary>
/// Pipeline ¨BÆJ¤¶­±
/// </summary>
public interface IMapPipelineStep
{
    string StepName { get; }
    PipelinePhase Phase { get; }
    int Priority { get; }
    bool Enabled { get; set; }
    
    void Execute(MapPipelineContext context);
}

public enum PipelinePhase
{
    DataGeneration = 0,
    TerrainDecision = 1,
    Visualization = 2
}
