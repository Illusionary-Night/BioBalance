using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

[CustomEditor(typeof(Creature))]
public class CreatureEditor : Editor
{
    // 分頁索引與名稱
    private int tabIndex = 0;
    private string[] tabNames = { "Dashboard", "Debug Tools" };

    // 內部測試變數 (持久化於 Editor 實例)
    private Vector2Int testPos = Vector2Int.zero;
    private float testAngle = 0f;

    public override void OnInspectorGUI()
    {
        Creature creature = (Creature)target;
        if (creature == null) return;

        // 1. 繪製導覽列
        tabIndex = GUILayout.Toolbar(tabIndex, tabNames);
        EditorGUILayout.Space(10);

        // 2. 根據選中分頁切換顯示邏輯
        switch (tabIndex)
        {
            case 0:
                DrawDashboardTab(creature);
                break;
            case 1:
                DrawDebugTab(creature);
                break;
        }

        // 運行模式下自動重繪以維持進度條動畫
        if (Application.isPlaying) Repaint();
    }

    #region --- Dashboard 分頁渲染 ---

    /// <summary>
    /// 繪製儀表板分頁：包含狀態條、行動冷卻與大腦狀態
    /// </summary>
    private void DrawDashboardTab(Creature creature)
    {
        DrawVitalStatusBars(creature);
        EditorGUILayout.Space(10);

        DrawActionIntelligence(creature);
        EditorGUILayout.Space(10);

        DrawBrainStateMonitor(creature);
    }

    /// <summary>
    /// 繪製核心生存數值進度條 (血量、飢餓、年齡)
    /// </summary>
    private void DrawVitalStatusBars(Creature creature)
    {
        EditorGUILayout.LabelField("Live Status Monitor", EditorStyles.boldLabel);

        // 血量 (動態變色)
        float healthPct = Mathf.Clamp01(creature.health / creature.maxHealth);
        Color hpColor = Color.Lerp(Color.red, Color.green, healthPct);
        DrawProgressBar("Health", creature.health, healthPct, hpColor);

        // 飢餓 (橘色)
        float hungerPct = Mathf.Clamp01(creature.hunger / creature.maxHunger);
        DrawProgressBar("Hunger", creature.hunger, hungerPct, new Color(1f, 0.6f, 0f));

        // 年齡 (灰色)
        float agePct = Mathf.Clamp01(creature.age / creature.lifespan);
        DrawProgressBar("Age", creature.age, agePct, Color.gray);
    }

    /// <summary>
    /// 繪製行動冷卻與決策權重系統
    /// </summary>
    private void DrawActionIntelligence(Creature creature)
    {
        // 防禦性檢查：確保大腦跟冷卻字典都已經準備好
        if (creature.GetStateMachine() == null || creature.GetActionCDList() == null)
        {
            EditorGUILayout.HelpBox("Creature Brain or Cooldowns not initialized.", MessageType.Warning);
            return;
        }
        EditorGUILayout.LabelField("Action Intelligence", EditorStyles.boldLabel);

        // 1. 通用全域冷卻 (Universal CD)
        int maxUCD = Mathf.Max(1, constantData.UNIVERSAL_ACTION_COOLDOWN);
        float ucdPct = Mathf.Clamp01((float)creature.actionCooldown / maxUCD);
        DrawProgressBar("Universal CD", creature.actionCooldown, ucdPct, Color.gray);

        EditorGUILayout.Space(5);

        // 2. 個別動作冷卻與條件檢查
        var actionCDs = creature.GetActionCDList();
        var actionMaxCDs = creature.GetActionMaxCDList();
        var debugCache = creature.GetStateMachine().DebugInfoCache;

        foreach (var action in actionMaxCDs)
        {
            ActionType type = action.Key;
            int currentCD = 0;
            actionCDs.TryGetValue(type, out currentCD);

            bool isMet = false;
            float weight = 0;
            if (debugCache.TryGetValue(type, out var info))
            {
                isMet = info.isConditionMet;
                weight = info.weight;
            }

            // 判斷狀態配色邏輯
            Color barColor;
            string label;
            float progress;

            if (currentCD > 0)
            {
                barColor = new Color(0.44f, 0.5f, 0.56f); // 冷卻中：石板藍
                label = $"{type} ({currentCD}t)";
                progress = Mathf.Clamp01((float)currentCD / action.Value);
            }
            else
            {
                progress = 1.0f; // 就緒狀態填滿條以顯示顏色
                if (isMet)
                {
                    barColor = new Color(0.53f, 0.66f, 0.42f); // 可執行：鼠尾草綠
                    label = $"{type} [W: {weight:F1}]";
                }
                else
                {
                    barColor = new Color(0.69f, 0.49f, 0.49f); // 邏輯阻斷：莫蘭迪紅
                    label = $"{type} [Blocked]";
                }
            }

            Rect rect = EditorGUILayout.GetControlRect(false, 18);
            DrawColoredProgressBar(rect, progress, label, barColor);
            EditorGUILayout.Space(2);
        }
    }

    /// <summary>
    /// 繪製大腦運行時的變數監測
    /// </summary>
    private void DrawBrainStateMonitor(Creature creature)
    {
        EditorGUILayout.LabelField("Brain State", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        // 保存原本的寬度
        float oldWidth = EditorGUIUtility.labelWidth;
        // 強制設定標題欄寬度為 120 像素 (視需求調整)
        EditorGUIUtility.labelWidth = 120;

        EditorGUILayout.LabelField($"Current Action:", creature.currentAction.ToString());
        EditorGUILayout.LabelField($"Distance:", creature.GetDistanceToDestination().ToString("F2"));
        EditorGUILayout.LabelField($"Stuck Times:", creature.GetMovementStuckTimes().ToString());
        EditorGUILayout.LabelField($"Attack Dir:", creature.underAttackDirection.ToString());

        string uuid = creature.enemy?.UUID ?? "None";
        string lastFive = uuid.Length >= 5 ? uuid.Substring(uuid.Length - 5) : uuid;
        EditorGUILayout.LabelField("Enemy ID (Last 5):", lastFive);

        // 恢復寬度，以免影響到後面的渲染
        EditorGUIUtility.labelWidth = oldWidth;

        EditorGUILayout.EndVertical();
    }

    #endregion

    #region --- Debug Tab 分頁渲染 ---

    /// <summary>
    /// 繪製上帝工具分頁：包含屬性修改與戰鬥測試
    /// </summary>
    private void DrawDebugTab(Creature creature)
    {
        DrawLifeManagementTools(creature);
        EditorGUILayout.Space(10);

        DrawSpecialStateTools(creature);
        EditorGUILayout.Space(10);

        DrawEvolutionTools(creature);
        EditorGUILayout.Space(10);

        DrawUnderAttackTest(creature);
        EditorGUILayout.Space(10);

        DrawIndicatorButton(creature);
    }

    private void DrawLifeManagementTools(Creature creature)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Life Management", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Full Restore"))
        {
            creature.SetHealth(creature.maxHealth);
            creature.SetHunger(creature.maxHunger);
        }
        if (GUILayout.Button("Set 40%"))
        {
            creature.SetHealth(creature.maxHealth * 0.4f);
            creature.SetHunger(creature.maxHunger * 0.4f);
        }
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("Kill Immediately", GUILayout.Height(25))) creature.Die();
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();
    }

    private void DrawSpecialStateTools(Creature creature)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Special States", EditorStyles.boldLabel);
        bool nextInvincible = EditorGUILayout.Toggle("Invincible Mode", creature.isInvincible);
        if (nextInvincible != creature.isInvincible) creature.SetInvincible(nextInvincible);
        EditorGUILayout.EndVertical();
    }

    private void DrawEvolutionTools(Creature creature)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Evolution & Time", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Age +10%")) creature.SetAge(creature.age + creature.lifespan * 0.1f);
        if (GUILayout.Button("Age -10%")) creature.SetAge(creature.age - creature.lifespan * 0.1f);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Reset All Cooldowns")) creature.ResetAllCooldowns();
        EditorGUILayout.EndVertical();
    }


    private void DrawUnderAttackTest(Creature creature)
    {
        EditorGUILayout.LabelField("Direction Test (Counter-Clockwise)", EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"Under Attack Direction: {creature.GetUnderAttackDirection()}");
        EditorGUILayout.LabelField($"Remain HP: {creature.health:F1}");

        // 1. Slider 調整：0度在右側(X+), 90度在上方(Y+)
        testAngle = EditorGUILayout.Slider("Test Angle (deg)", testAngle, 0, 360);

        // 2. 繪製視覺化小圓盤
        Rect rect = GUILayoutUtility.GetRect(80, 80);
        if (Event.current.type == EventType.Repaint)
        {
            Vector2 center = rect.center;
            Handles.color = new Color(0.3f, 0.3f, 0.3f);
            Handles.DrawWireDisc(center, Vector3.forward, 35f);

            // 核心邏輯：將角度轉換為弧度
            float rad = testAngle * Mathf.Deg2Rad;

            // 在 Unity Editor UI 中，Y 軸是向下增長的，但 Handles 繪製座標系略有不同
            // 為了確保「逆時針為正」，我們計算出方向向量
            Vector2 dir = new Vector2(Mathf.Cos(rad), -Mathf.Sin(rad)); // 這裡加負號是因為 GUI 座標系 Y 軸向下

            Handles.color = Color.red;
            Handles.DrawLine(center, center + dir * 35f);

            // 畫一個小箭頭或點代表方向頭部
            Handles.DrawSolidDisc(center + dir * 35f, Vector3.forward, 3f);
        }

        // 3. 測試按鈕
        if (GUILayout.Button("Hurt from that Angle"))
        {
            float rad = testAngle * Mathf.Deg2Rad;
            // 在世界座標中，+Y 通常是往上，所以不需要負號
            Vector2 worldDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector2 spawnPos = (Vector2)creature.transform.position + worldDir;
            Debug.Log("Hurt from "+worldDir);
            creature.Hurt(10, spawnPos);
        }
    }


    private void DrawIndicatorButton(Creature creature)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("World Visualizer", EditorStyles.boldLabel);

        if (GUILayout.Button("Show Destination In Scene", GUILayout.Height(30)))
        {
            Vector2Int dest = creature.GetMovementDestination();

            if (IndicatorController.Instance != null)
            {
                var destIndicator = IndicatorController.Instance.GetIndicator<DestinationIndicator>();
                if (destIndicator != null)
                {
                    destIndicator.SetTarget(creature);
                    destIndicator.Show();
                }
                else
                {
                    Debug.Log("destIndicator is null");
                }

            }
            else
            {
                Debug.LogWarning("IndicatorController not found in scene!");
            }
        }

        if (GUILayout.Button("Hide Indicator"))
        {
            var destIndicator = IndicatorController.Instance.GetIndicator<DestinationIndicator>();
            if (destIndicator != null)
            {
                destIndicator.Hide();
            }
        }
        EditorGUILayout.EndVertical();
    }

    #endregion

    #region --- 通用繪製工具 ---

    /// <summary>
    /// 封裝好的進度條繪製邏輯
    /// </summary>
    private void DrawProgressBar(string label, float current, float pct, Color color)
    {
        Rect rect = EditorGUILayout.GetControlRect(false, 20);
        DrawColoredProgressBar(rect, pct, $"{label}: {current:F1} ({pct * 100:F0}%)", color);
        EditorGUILayout.Space(2);
    }

    private void DrawColoredProgressBar(Rect rect, float value, string text, Color barColor)
    {
        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f)); // 背景
        Rect fillRect = new Rect(rect.x, rect.y, rect.width * value, rect.height);
        EditorGUI.DrawRect(fillRect, barColor); // 填充

        GUIStyle textStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        textStyle.normal.textColor = Color.white;
        EditorGUI.LabelField(rect, text, textStyle);
    }

    #endregion
}