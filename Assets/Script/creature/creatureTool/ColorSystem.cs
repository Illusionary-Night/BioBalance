using UnityEngine;

public static class ColorSystem
{
    // 定義六角形的基礎顏色 (這在 C# 裡定義)
    private Color[] _baseColors = new Color[] {
        new Color(1f, 0f, 0f),    // 紅
        new Color(1f, 0.5f, 0f),  // 橙
        new Color(1f, 1f, 0f),    // 黃
        new Color(0f, 1f, 0f),    // 綠
        new Color(0f, 0f, 1f),    // 藍
        new Color(0.5f, 0f, 1f)   // 紫
    };

    public static void InitializeColorGenes(CreatureData data, float[] parent1Color, float[] parent2Color)
    {
        float mutationWeight = 0.1f;
        float inheritWeight = 1f - mutationWeight; // 0.9f

        // 1. 產生嚴格的 10% 隨機突變比例 (避免歸一化稀釋父母權重)
        float[] randomMutation = new float[6];
        float randomTotal = 0;
        for (int i = 0; i < 6; i++)
        {
            randomMutation[i] = UnityEngine.Random.value;
            randomTotal += randomMutation[i];
        }

        // 2. 依照 0、1、2 雙親狀態進行色彩分配
        for (int i = 0; i < 6; i++)
        {
            // 算出該顏色在此次突變中實際分到的比例 (加總保證等於 0.1)
            float strictRandomPart = (randomMutation[i] / randomTotal) * mutationWeight;

            float baseColorPart = 0f;

            if (parent1Color == null && parent2Color == null)
            {
                // 【初代個體】：0 個雙親，使用隨機色彩
                float[,] baseColor = new float[,] {
                    {1f, 0f, 0f, 0f, 0f, 0f},
                    {0f, 0f, 1f, 0f, 0f, 0f},
                    {0f, 0f, 0f, 0f, 1f, 0f}
                };
                baseColorPart = baseColor[UnityEngine.Random.Range(0, 3), i] * inheritWeight;
            }
            else if (parent2Color == null)
            {
                // 【無性生殖/孤雌生殖】：1 個雙親，繼承單親 90% 色彩
                baseColorPart = parent1Color[i] * inheritWeight;
            }
            else
            {
                // 【有性生殖】：2 個雙親，父母各佔 45% 色彩
                baseColorPart = (parent1Color[i] * (inheritWeight / 2f)) + (parent2Color[i] * (inheritWeight / 2f));
            }

            // 組合最終基因
            // 因為 inheritWeight (0.9) + mutationWeight (0.1) = 1.0
            // 所以組合出來的六色比例總和必定為 1，無需再次執行全陣列歸一化
            data.colorGenes[i] = baseColorPart + strictRandomPart;
        }
    }

    public static void UpdateColorGenes(CreatureData data)
    {
        CheckWandering();
        if (data.isWandering)
        {
            float target = 1f / 6f; // 0.166... 平均值
            for (int i = 0; i < 6; i++)
            {
                data.colorGenes[i] = Mathf.Lerp(data.colorGenes[i], target, Time.deltaTime * data.fadeSpeed);
            }
            UpdateVisuals(data); // 更新顏色展現
        }
    }
    private SpriteRenderer _spriteRenderer;



    public static void UpdateVisuals(CreatureData data)
    {
        // 建立暫存陣列，保護原始基因資料 (總和維持 1)
        float[] renderColors = new float[6];
        System.Array.Copy(data.colorGenes, renderColors, 6);

        // 索引對應：0:紅, 1:橙, 2:黃, 3:綠, 4:藍, 5:紫

        // ==========================================
        // 步驟 1：分離律 (1 橘 -> 1 紅 + 1 黃)
        // ==========================================
        renderColors[0] += renderColors[1] + renderColors[5]; // 紅 += 橙 + 紫
        renderColors[2] += renderColors[1] + renderColors[3]; // 黃 += 橙 + 綠
        renderColors[4] += renderColors[3] + renderColors[5]; // 藍 += 綠 + 紫

        // 次要色概念已完全釋放至主要色，將其歸零
        renderColors[1] = 0f;
        renderColors[3] = 0f;
        renderColors[5] = 0f;

        // ==========================================
        // 步驟 2：抵銷律 (紅、黃、藍 1:1:1 相消)
        // ==========================================
        float minPrimary = Mathf.Min(renderColors[0], renderColors[2], renderColors[4]);
        renderColors[0] -= minPrimary;
        renderColors[2] -= minPrimary;
        renderColors[4] -= minPrimary;

        // 此時，紅、黃、藍必定至少有一者歸零。

        // ==========================================
        // 步驟 3：結合律 (1 紅 + 1 黃 -> 1 橙)
        // ==========================================
        if (renderColors[0] > 0 && renderColors[2] > 0)
        {
            // 剩下紅與黃 -> 聚合為橙色
            float combineAmt = Mathf.Min(renderColors[0], renderColors[2]);
            renderColors[1] += combineAmt;
            renderColors[0] -= combineAmt;
            renderColors[2] -= combineAmt;
        }
        else if (renderColors[2] > 0 && renderColors[4] > 0)
        {
            // 剩下黃與藍 -> 聚合為綠色
            float combineAmt = Mathf.Min(renderColors[2], renderColors[4]);
            renderColors[3] += combineAmt;
            renderColors[2] -= combineAmt;
            renderColors[4] -= combineAmt;
        }
        else if (renderColors[4] > 0 && renderColors[0] > 0)
        {
            // 剩下藍與紅 -> 聚合為紫色
            float combineAmt = Mathf.Min(renderColors[4], renderColors[0]);
            renderColors[5] += combineAmt;
            renderColors[4] -= combineAmt;
            renderColors[0] -= combineAmt;
        }

        // ==========================================
        // 最終輸出與歸一化 (確保顯示亮度正常)
        // ==========================================
        // 因為 1->2 與 2->1 的轉換會導致數值總和膨脹或收縮
        // 為了讓 SpriteRenderer 正確顯示，我們需要找出目前剩下的總能量並重新分配
        float remainingEnergy = 0f;
        for (int i = 0; i < 6; i++)
        {
            remainingEnergy += renderColors[i];
        }

        Color finalColor = Color.black;

        // 若完全抵銷 (例如原本剛好是紅黃藍各 1/3)，呈現無色/灰色
        if (remainingEnergy <= 0.0001f)
        {
            finalColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        }
        else
        {
            // 將剩餘的純化顏色依照比例轉為 RGB
            for (int i = 0; i < 6; i++)
            {
                renderColors[i] /= remainingEnergy; // 歸一化到 0~1 範圍
                finalColor += _baseColors[i] * renderColors[i];
            }
        }

        finalColor.a = 1f;
        _spriteRenderer.color = finalColor;
        // Debug.Log("Final Color: " + finalColor + " sprite: " + _spriteRenderer.sprite.name);
    }
    //TODO: Wander這個字要改掉，跟action裡面的一個東西重複了，會讓人誤會。
    private void CheckWandering()
    {
        // 1. 頻率限制 (Timer)
        _checkTimer -= Time.deltaTime;
        if (_checkTimer > 0f) return;

        // 重置計時器，加入一點隨機值避免所有生物在同一個 Frame 進行運算 (效能優化)
        _checkTimer = checkInterval + Random.Range(-0.1f, 0.1f);

        // 4. 更新走散狀態
        isWandering = !Perception.Creatures.HasTarget(this, speciesID, 0.5f);
    }
    private bool IsSimilarColor(Creature other)
    {
        float difference = 0f;

        // 將自己的六色與對方的六色逐一相減取絕對值
        for (int i = 0; i < 6; i++)
        {
            difference += Mathf.Abs(this.colorGenes[i] - other.colorGenes[i]);
        }

        // 說明：
        // 如果完全一樣，difference = 0
        // 如果完全極端 (例如我是 100% 紅，你是 100% 藍)，difference = 2
        // 如果 threshold 設為 0.4，代表允許兩者在六色分配上有 20% 的偏移
        return difference <= similarityThreshold;
    }
}
