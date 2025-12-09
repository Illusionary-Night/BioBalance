using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewSimpleTileData", menuName = "BioBalance/Simple Tile Data")]
public class SimpleTileData : ScriptableObject
{
    public TerrainType terrainType;

    [Header("Tile")]
    [Tooltip("放入對應的 Tile Asset")]
    public TileBase tile; // 只有1個圖塊
}