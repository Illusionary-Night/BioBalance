using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewDualGridTile", menuName = "BioBalance/Dual Grid Tile Data")]
public class DualGridTileData : ScriptableObject
{
    public TerrainType terrainType;

    [Tooltip("優先級：數值越大，越會蓋在別人上面 (例如：岩石 > 草 > 沙 > 水)")]
    public int priority;

    [Header("16 Tiles (Dual-Grid)")]
    [Tooltip("請依照 0-15 的順序放入對應的 Tile Asset")]
    // 順序通常是 bitmask: TL=8, TR=4, BL=2, BR=1
    public TileBase[] tiles = new TileBase[16];
}