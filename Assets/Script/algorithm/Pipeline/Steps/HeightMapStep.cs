using UnityEngine;

/// <summary>
/// 高度圖生成 - 還原舊版 TerrainGenerator 的 Perlin Noise 邏輯
/// </summary>
[System.Serializable]
public class HeightMapStep : IMapPipelineStep
{
    public const string LAYER_NAME = "Height";
    
    [Header("Noise Settings")]
    public float noiseScale = 20f;
    public int octaves = 4;
    [Range(0f, 1f)] public float persistence = 0.5f;
    public float lacunarity = 2f;
    
    [SerializeField] private bool enabled = true;
    
    public string StepName => "Height Map";
    public PipelinePhase Phase => PipelinePhase.DataGeneration;
    public int Priority => 0;
    public bool Enabled { get => enabled; set => enabled = value; }
    
    public void Execute(MapPipelineContext ctx)
    {
        ctx.CreateLayer(LAYER_NAME);
        
        for (int x = 0; x < ctx.Width; x++)
        {
            for (int y = 0; y < ctx.Height; y++)
            {
                float value = GetPerlinNoiseValue(x, y, ctx.NoiseOffset);
                ctx.SetValue(LAYER_NAME, x, y, value);
            }
        }
    }
    
    private float GetPerlinNoiseValue(int x, int y, Vector2 offset)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseValue = 0f;
        float maxPossibleHeight = 0f;
        
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x / noiseScale) * frequency + offset.x * frequency;
            float sampleY = (y / noiseScale) * frequency + offset.y * frequency;
            
            float perlin = Mathf.PerlinNoise(sampleX, sampleY);
            noiseValue += perlin * amplitude;
            maxPossibleHeight += amplitude;
            
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        
        return maxPossibleHeight > 0 
            ? Mathf.Clamp01(noiseValue / maxPossibleHeight) 
            : Mathf.Clamp01(noiseValue);
    }
}