
using UnityEngine;
/// <summary>
/// 8 方向列舉
/// </summary>
public enum Direction
{
    None, East, Northeast, North, Northwest, West, Southwest, South, Southeast
}
public static class DirectionHelper
{


    /// <summary>
    /// 方向角度判定所使用的臨界值常數
    /// </summary>
    public static class DirectionAngles
    {
        public const float AngleStep = 45.0f;
        public const float HalfAngleStep = 22.5f;

        // 8 方向切分臨界值 (最大臨界點)
        public const float EastMax = 22.5f;
        public const float NortheastMax = 67.5f;
        public const float NorthMax = 112.5f;
        public const float NorthwestMax = 157.5f;
        public const float WestMax = 202.5f;
        public const float SouthwestMax = 247.5f;
        public const float SouthMax = 292.5f;
        public const float SoutheastMax = 337.5f;
    }

    /// <summary> 
    /// 將向量轉換為 8 方向列舉，以 45 度角為一個判斷區間 
    /// </summary>
    public static Direction GetDirectionFromVector(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.001f) return Direction.None;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360f;

        // 🌟 直接讀取封裝好的常數判定，完全沒有硬編碼魔術數字
        if (angle >= DirectionAngles.SoutheastMax || angle < DirectionAngles.EastMax)
            return Direction.East;
        if (angle >= DirectionAngles.EastMax && angle < DirectionAngles.NortheastMax)
            return Direction.Northeast;
        if (angle >= DirectionAngles.NortheastMax && angle < DirectionAngles.NorthMax)
            return Direction.North;
        if (angle >= DirectionAngles.NorthMax && angle < DirectionAngles.NorthwestMax)
            return Direction.Northwest;
        if (angle >= DirectionAngles.NorthwestMax && angle < DirectionAngles.WestMax)
            return Direction.West;
        if (angle >= DirectionAngles.WestMax && angle < DirectionAngles.SouthwestMax)
            return Direction.Southwest;
        if (angle >= DirectionAngles.SouthwestMax && angle < DirectionAngles.SouthMax)
            return Direction.South;
        if (angle >= DirectionAngles.SouthMax && angle < DirectionAngles.SoutheastMax)
            return Direction.Southeast;

        return Direction.None;
    }
}