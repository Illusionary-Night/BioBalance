using UnityEngine;
//主要是用來將物種的標本初始化與標本室管理
//TODO: 改成從CreatureBase去拉，而不是從Species去找
//TODO: 但應該是寫CreatureBase的時候再修改，甚至可能不需要
public static class VisualTemplateManager
{
    private static Transform _templateHolder;

    private static void EnsureHolderExists()
    {
        if (_templateHolder == null)
        {
            GameObject holderObj = new GameObject("--- [Hidden Species Templates] ---");
            Object.DontDestroyOnLoad(holderObj);
            holderObj.SetActive(false); // 標本室預設隱藏
            _templateHolder = holderObj.transform;
        }
    }

    /// <summary>
    /// 物種的gameObject標本初始化
    /// </summary>
    public static void InitializeSpeciesTemplate(Species species)
    {
        EnsureHolderExists();

        // 第一種方式：預先做好的 Prefab
        if (species.visualTemplate != null)
        {
            // 我們把 Prefab 實例化一份放進隱藏標本室，確保後續操作的統一是場景物件
            GameObject template = Object.Instantiate(species.visualTemplate, _templateHolder);
            template.name = $"Template_{species.creatureBase}";
            species.visualTemplate = template;
            return;
        }

        // 第二種方式：自動包裝程序
        // 適用於沒拉 Prefab 的物種 或 Runtime 新物種
        GameObject autoTemplate = new GameObject($"Template_{species.creatureBase}");
        autoTemplate.transform.SetParent(_templateHolder);

        // 自動加入 SpriteRenderer
        var sr = autoTemplate.AddComponent<SpriteRenderer>();

        // 完美接回你們原本的 Resources.Load 寫法
        string spriteName = species.creatureBase.ToString();
        Sprite loadedSprite = Resources.Load<Sprite>($"Sprites/{spriteName}");

        if (loadedSprite != null)
        {
            sr.sprite = loadedSprite;

            // 自動計算半徑
            var col = autoTemplate.AddComponent<CircleCollider2D>();
            float maxDim = Mathf.Max(loadedSprite.bounds.size.x, loadedSprite.bounds.size.y);
            col.radius = maxDim * 0.5f;
        }
        else
        {
            // 如果資料夾裡真的沒有這張圖，報錯提示
            Debug.LogError($"找不到對應圖片: Sprites/{spriteName}");
        }

        // 掛上 Creature 外殼
        autoTemplate.AddComponent<Creature>();

        // 自動設定 Layer
        AutoSetLayer(autoTemplate);

        // 6. 將做好的自動模具交還給 Species，以後生成就直接複製它
        species.visualTemplate = autoTemplate;
    }

    public static void AutoSetLayer(GameObject obj)
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

    public static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursive(child.gameObject, layer);
        }
    }
}