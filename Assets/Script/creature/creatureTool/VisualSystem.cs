using UnityEngine;

public class VisualSystem
{
    //TODO: 可能要考慮掛到Species上面
    private void SetCreatureSprite(CreatureBase baseType)
    {

        // 1. 將 Enum 轉為字串 (例如 "Slime")
        string spriteName = baseType.ToString();

        // 2. 從 Resources 加載 (路徑需放在 Resources/Sprites/ 下)
        Sprite loadedSprite = Resources.Load<Sprite>($"Sprites/{spriteName}");

        if (loadedSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = loadedSprite;

            var col = GetComponent<CircleCollider2D>();
            if (col != null)
            {
                // 讓它自動抓！
                // 取圖片寬高之中較大的一半作為半徑，完美貼合任何形狀的生物圖片
                float maxDim = Mathf.Max(loadedSprite.bounds.size.x, loadedSprite.bounds.size.y);
                col.radius = maxDim * 0.5f;
            }
        }
        else
        {
            Debug.LogError($"找不到對應圖片: Sprites/{spriteName}");
        }
    }
    //TODO: 有點忘記他為什麼要存在的東西
    private void AutoSetLayer(GameObject obj)
    {
        // 將名稱轉為索引編號 (例如 "Creature" 是第 6 層，layerIndex 就會是 6)
        int layerIndex = LayerMask.NameToLayer("Creature");

        if (layerIndex == -1)
        {
            Debug.LogError("找不到名為 'Creature' 的 Layer，請先在選單中手動建立！");
            return;
        }

        // 設置該物件及其所有子物件的 Layer
        SetLayerRecursive(obj, layerIndex);
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}
