using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("攝影機移動速度")]
    public float panSpeed = 20f;
    [Tooltip("攝影機邊緣緩衝 (避免移出地圖太遠)")]
    public float panBorderThickness = 10f;
    [Tooltip("是否限制攝影機在地圖範圍內")]
    public bool limitToMap = true;

    [Header("Zoom Settings")]
    [Tooltip("滾輪縮放速度")]
    public float zoomSpeed = 20f; // 數值越大縮放越快
    [Tooltip("最小縮放 (拉得最近)")]
    public float minZoom = 5f;
    [Tooltip("最大縮放 (拉得最遠)")]
    public float maxZoom = 20f;

    private Camera cam;
    private float targetZoom;

    void Start()
    {
        cam = GetComponent<Camera>();
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        // 1. 處理移動 (WASD 或 方向鍵)
        HandleMovement();

        // 2. 處理縮放 (滑鼠滾輪)
        HandleZoom();
    }

    void HandleMovement()
    {
        Vector3 pos = transform.position;

        // 獲取鍵盤輸入 (Horizontal = A/D/左/右, Vertical = W/S/上/下)
        // 使用 GetAxisRaw 可以獲得更靈敏的反應，GetAxis 會有平滑緩衝
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // 計算移動量
        Vector3 move = new Vector3(h * panSpeed * Time.deltaTime, v * panSpeed * Time.deltaTime, 0);
        pos += move;

        // 限制移動範圍 (Clamp)
        if (limitToMap && TerrainGenerator.Instance != null)
        {
            // 獲取地圖寬高
            float mapW = TerrainGenerator.Instance.mapWidth;
            float mapH = TerrainGenerator.Instance.mapHeight;

            // 限制 X 和 Y 座標
            // 這裡假設地圖從 (0,0) 開始，所以範圍是 0 到 mapWidth/Height
            // 我們稍微加一點緩衝 (panBorderThickness)，讓玩家可以看到邊緣外面一點點
            pos.x = Mathf.Clamp(pos.x, -panBorderThickness, mapW + panBorderThickness);
            pos.y = Mathf.Clamp(pos.y, -panBorderThickness, mapH + panBorderThickness);
        }

        transform.position = pos;
    }

    void HandleZoom()
    {
        // 獲取滾輪輸入 (正值 = 往上滾/放大，負值 = 往下滾/縮小)
        float scrollData = Input.GetAxis("Mouse ScrollWheel");

        // 根據滾輪方向調整目標縮放值
        // 注意：OrthographicSize 越小 = 畫面越近(放大)，越大 = 畫面越遠(縮小)
        // 所以我們要 "減去" scrollData
        targetZoom -= scrollData * zoomSpeed;

        // 限制縮放範圍
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        // 平滑縮放效果 (Lerp)
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * 10f);
    }
}