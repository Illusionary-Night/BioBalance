using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public class MapPipeline : MonoBehaviour
{
    public static MapPipeline Instance { get; private set; }
    
    [Header("Map Settings")]
    [SerializeField] private int mapWidth = 500;
    [SerializeField] private int mapHeight = 500;
    [SerializeField] private int mapSeed = 1337;
    
    [Header("Debug")]
    [SerializeField] private bool logTiming = true;
    
    private readonly List<IMapPipelineStep> steps = new List<IMapPipelineStep>();
    private MapPipelineContext context;
    
    // === 公開屬性 ===
    public MapPipelineContext Context => context;
    public TerrainMap OutputMap => context?.OutputMap;
    public int Width => mapWidth;
    public int Height => mapHeight;
    public int Seed { get => mapSeed; set => mapSeed = value; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    
    // 步驟管理
    public void AddStep(IMapPipelineStep step)
    {
        steps.Add(step);
        SortSteps();
    }
    
    public void AddSteps(params IMapPipelineStep[] newSteps)
    {
        steps.AddRange(newSteps);
        SortSteps();
    }
    
    public void RemoveStep<T>() where T : IMapPipelineStep
    {
        steps.RemoveAll(s => s is T);
    }
    
    public T GetStep<T>() where T : class, IMapPipelineStep
    {
        return steps.OfType<T>().FirstOrDefault();
    }
    
    public void ClearSteps() => steps.Clear();
    
    public IReadOnlyList<IMapPipelineStep> GetAllSteps() => steps.AsReadOnly();
    
    private void SortSteps()
    {
        steps.Sort((a, b) =>
        {
            int phase = a.Phase.CompareTo(b.Phase);
            return phase != 0 ? phase : a.Priority.CompareTo(b.Priority);
        });
    }
    
    // 執行
    public void Execute()
    {
        context = new MapPipelineContext(mapWidth, mapHeight, mapSeed);
        
        // 產生 NoiseOffset (相容舊版)
        Random.InitState(mapSeed);
        context.NoiseOffset = new Vector2(
            Random.Range(-10000f, 10000f),
            Random.Range(-10000f, 10000f)
        );
        
        var total = Stopwatch.StartNew();
        
        foreach (var step in steps)
        {
            if (!step.Enabled) continue;
            
            var sw = Stopwatch.StartNew();
            step.Execute(context);
            sw.Stop();
            
            if (logTiming)
                UnityEngine.Debug.Log($"[{step.Phase}] {step.StepName}: {sw.ElapsedMilliseconds}ms");
        }
        
        total.Stop();
        UnityEngine.Debug.Log($"<color=#88ff88>Pipeline 完成: {total.ElapsedMilliseconds}ms</color>");
    }
    
    public void RandomizeSeed()
    {
        mapSeed = Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Debug.Log($"New seed: {mapSeed}");
    }
}
