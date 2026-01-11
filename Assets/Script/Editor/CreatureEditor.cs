using static UnityEngine.GraphicsBuffer;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using static Perception;

[CustomEditor(typeof(Creature))]
public class CreatureEditor : Editor
{
    // 用來紀錄目前選中的分頁索引
    private int tabIndex = 0;
    private string[] tabNames = { "Dashboard", "Genetic Data", "Debug Tools", "Event Subscription" };

    // 一些function內部的變數，為了不被刷新只能定在這裡
    private Vector2Int testPos = Vector2Int.zero;
    private float testAngle = 0f;
    public override void OnInspectorGUI()
    {
        Creature creature = (Creature)target;

        // 1. 繪製頂部分頁按鈕 (Toolbar)
        tabIndex = GUILayout.Toolbar(tabIndex, tabNames);

        EditorGUILayout.Space(10);

        // 2. 根據選中的分頁顯示內容
        switch (tabIndex)
        {
            case 0:
                DrawDashboardTab(creature);
                break;
            case 1:
                DrawGeneticTab(creature);
                break;
            case 2:
                DrawDebugTab(creature);
                DrawIndicatorButton(creature);
                break;
            case 3:
                DrawEventSubscriptionMonitor(creature);
                break;
        }

        // 執行模式下自動刷新
        if (Application.isPlaying) Repaint();
    }

    // --- 第一頁：可視化儀表板 ---
    private void DrawDashboardTab(Creature creature)
    {
        EditorGUILayout.LabelField("Live Status Monitor", EditorStyles.boldLabel);

        // --- 1. 血量進度條 (動態變色：綠 -> 紅) ---
        float healthPct = Mathf.Clamp01(creature.Health / creature.BaseHealth);
        Color hpColor = Color.Lerp(Color.red, Color.green, healthPct);
        Rect hpRect = EditorGUILayout.GetControlRect(false, 20);
        DrawColoredProgressBar(hpRect, healthPct, $"Health: {creature.Health:F1} ({healthPct * 100:F0}%)", hpColor);

        EditorGUILayout.Space(5);

        // --- 2. 飢餓度進度條 (固定橘色) ---
        float hungerPct = Mathf.Clamp01(creature.Hunger / creature.MaxHunger);
        Color hungerColor = new Color(1f, 0.6f, 0f); // 橘色
        Rect hungerRect = EditorGUILayout.GetControlRect(false, 20);
        DrawColoredProgressBar(hungerRect, hungerPct, $"Hunger: {creature.Hunger:F1} ({hungerPct * 100:F0}%)", hungerColor);

        EditorGUILayout.Space(5);

        // --- 3. 年齡進度條 (固定灰色) ---
        float agePct = Mathf.Clamp01(creature.Age / creature.Lifespan);
        Rect ageRect = EditorGUILayout.GetControlRect(false, 20);
        DrawColoredProgressBar(ageRect, agePct, $"Age: {creature.Age:F0} ({agePct * 100:F0}%)", Color.gray);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Action Cooldowns", EditorStyles.boldLabel);

        // --- 4. 行動進度條 ---
        var actionCDs = creature.GetActionCDList(); 
        var actionMaxCDs = creature.GetActionMaxCDList();

        if (actionCDs == null || actionCDs.Count == 0)
        {
            EditorGUILayout.HelpBox("No active action cooldowns.", MessageType.None);
            return;
        }

        foreach(var action in actionMaxCDs)
        {
            int maxCD = action.Value;
            int CD = 0;
            actionCDs.TryGetValue(action.Key, out CD);
            float progress = Mathf.Clamp01( (float)CD / maxCD);

            Rect rect = EditorGUILayout.GetControlRect(false, 18);

            // 使用冷調的紫色或藍色來區分於血條
            Color cdColor = new Color(0.5f, 0.5f, 1f);

            DrawColoredProgressBar(rect, progress, $"{action.Key}: {CD} Ticks / {maxCD} Ticks", cdColor);
            EditorGUILayout.Space(2);
        }
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Brain State", EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"Global Action CD: {creature.ActionCooldown}");

        EditorGUILayout.LabelField($"Current Action: {creature.CurrentAction}");

        EditorGUILayout.LabelField($"Distance: {creature.GetDistanceToDestination()}");

        EditorGUILayout.LabelField($"StuckTimes: {creature.GetMovementStuckTimes()}");

        // 讓畫面在執行時動態刷新
        if (Application.isPlaying) Repaint();
    }

    // --- 第二頁：原始遺傳資料 ---
    private void DrawGeneticTab(Creature creature)
    {
        EditorGUILayout.LabelField("Base Genetic Attributes", EditorStyles.boldLabel);

        // 顯示原本的 Inspector 內容（或者你手動排版變數）
        // 如果只想顯示部分，可以用 EditorGUILayout.PropertyField
        base.OnInspectorGUI();
    }

    // --- 第三頁：上帝權限工具 ---
    private void DrawDebugTab(Creature creature)
    {
        // --- Section: Life & Health ---
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Life Management", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Full Restore"))
        {
            creature.Health = creature.BaseHealth;
            creature.Hunger = creature.MaxHunger;
        }

        if (GUILayout.Button("Set Hunger 40%"))
        {
            creature.Hunger = creature.MaxHunger * 0.4f;
        }

        if (GUILayout.Button("Set Health 40%"))
        {
            creature.Health = creature.BaseHealth * 0.4f;
        }
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("Kill Immediately", GUILayout.Height(25)))
        {
            creature.Die();
        }
        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // --- Section: Special States ---
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Special States", EditorStyles.boldLabel);

        // 1. 先用一個暫存變數讀取目前的勾選狀態，並獲取使用者的點擊結果
        bool nextInvincible = EditorGUILayout.Toggle("Invincible Mode", creature.IsInvincible);

        // 2. 判斷使用者有沒有「點擊」動作（數值是否改變）
        if (nextInvincible != creature.IsInvincible)
        {
            // 3. 透過你的接口來修改數值，這樣可以確保如果有任何 Set 邏輯（例如特效開關）也會被觸發
            creature.SetInvincible(nextInvincible);
        }
        EditorGUILayout.HelpBox("God Mode: No HP loss, No Hunger loss, No Death from old age.", MessageType.None);
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // --- Section: Time & Cycles ---
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Time & Evolution", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Age +10%"))
        {
            creature.Age += creature.Lifespan * 0.1f;
            creature.Age = Mathf.Min(creature.Age, creature.Lifespan);
        }
        if (GUILayout.Button("Age -10%"))
        {
            creature.Age -= creature.Lifespan * 0.1f;
            creature.Age = Mathf.Max(creature.Age, 0);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Reset All Cooldowns"))
        {
            creature.ResetAllCooldowns(); // Call the method we planned to add in Creature.cs
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // --- Section: AI & Navigation ---
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("AI Intervention", EditorStyles.boldLabel);

        creature.CurrentAction = (ActionType)EditorGUILayout.EnumPopup("Current Action", creature.CurrentAction);

        testPos = EditorGUILayout.Vector2IntField("Target Coordinates", testPos);
        if (GUILayout.Button("Force Move to Target"))
        {
            creature.MoveTo(testPos);
        }
        EditorGUILayout.EndVertical();

        DrawUnderAttackTest(creature);
    }

    private void DrawUnderAttackTest(Creature creature) {

        EditorGUILayout.LabelField("Direction Test", EditorStyles.boldLabel);

        EditorGUILayout.LabelField($"Under Attack Direction: {creature.GetUnderAttackDirection()}");
        EditorGUILayout.LabelField($"Remain HP: {creature.Health}");

        // 1. 使用滑桿調整角度
        testAngle = EditorGUILayout.Slider("Test Angle", testAngle, 0, 360);

        // 2. 繪製一個簡易的視覺化小圓盤
        Rect rect = GUILayoutUtility.GetRect(80, 80);
        if (Event.current.type == EventType.Repaint)
        {
            Vector2 center = rect.center;
            Handles.color = Color.grey;
            Handles.DrawWireDisc(center, Vector3.forward, 35f); // 畫圓圈

            // 畫出指針
            float rad = testAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Handles.color = Color.red;
            Handles.DrawLine(center, center + dir * 35f);
        }

        if (GUILayout.Button("Hurt from that Angle"))
        {
            float rad = testAngle * Mathf.Deg2Rad;
            Vector2 spawnPos = (Vector2)creature.transform.position + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            creature.Hurt(10, spawnPos);
        }
        if (GUILayout.Button("Hurt without Direction"))
        {
            creature.Hurt(10);
        }

        // 讓畫面在執行時動態刷新
        if (Application.isPlaying) Repaint();
    }
    private void DrawColoredProgressBar(Rect rect, float value, string text, Color barColor)
    {
        // 畫背景 (深灰色)
        EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f));

        // 畫填充層 (使用傳進來的顏色)
        Rect fillRect = new Rect(rect.x, rect.y, rect.width * value, rect.height);
        EditorGUI.DrawRect(fillRect, barColor);

        // 設定文字樣式 (置中、粗體、白色)
        GUIStyle textStyle = new GUIStyle();
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.white;
        textStyle.fontStyle = FontStyle.Bold;

        // 畫文字
        EditorGUI.LabelField(rect, text, textStyle);
    }
    private void DrawEventSubscriptionMonitor(Creature creature)
    {
        var sm = creature.GetStateMachine(); // 假設你公開了狀態機獲取

        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.LabelField("🔗 Event Lifecycle Monitor", EditorStyles.boldLabel);

        // 1. Context 狀態
        Color contextColor = sm.IsExecuting ? Color.green : Color.gray;
        GUI.color = contextColor;
        EditorGUILayout.LabelField($"Context Status: {(sm.IsExecuting ? "ACTIVE" : "IDLE")}");
        GUI.color = Color.white;

        // 2. 訂閱狀況
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Movement Callback:", GUILayout.Width(120));
        if (sm.HasMovementCallback)
        {
            GUI.color = Color.cyan;
            EditorGUILayout.LabelField("CONNECTED [✓]", EditorStyles.boldLabel);
        }
        else
        {
            GUI.color = Color.red;
            EditorGUILayout.LabelField("DISCONNECTED [X]");
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

        // 3. 追蹤列表 (registeredCallbacks)
        // 透過反射拿到 sm.registeredCallbacks.Count
        int callbackCount = GetPrivateFieldCount(sm, "registeredCallbacks");
        EditorGUILayout.LabelField($"Pending Callbacks: {callbackCount}");

        if (sm.IsExecuting && !sm.HasMovementCallback && sm.CurrentActionName == "Move")
        {
            EditorGUILayout.HelpBox("CRITICAL: Logic Deadlock! Action is Move but no Callback is registered!", MessageType.Error);
        }

        EditorGUILayout.EndVertical();
    }
    private int GetPrivateFieldCount(object obj, string fieldName)
    {
        if (obj == null) return 0;

        // 取得該物件的類別型態
        System.Type type = obj.GetType();

        // 尋找私有變數
        FieldInfo field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

        if (field != null)
        {
            // 取得該變數的內容並轉型為 ICollection (List 繼承自它，有 Count 屬性)
            var value = field.GetValue(obj) as ICollection;
            return value?.Count ?? 0;
        }

        return 0;
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
}
