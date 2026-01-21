using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

[CustomEditor(typeof(Species))]
public class SpeciesEditor : Editor
{
    private ActionType selectedActionToCD; // 在類別層級宣告
    Species species;
    public override void OnInspectorGUI()
    {
        species = (Species)target;
        // 加上這行可以自動處理多數欄位的 Undo
        serializedObject.Update();


        // 檢測 GUI 是否有變動
        EditorGUI.BeginChangeCheck();

        // --- 1. 繪製 DNA 設計師標題 ---
        EditorGUILayout.LabelField("Species DNA Designer", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // --- 2. 基礎數值 ---
        DrawBaseAttributes();

        EditorGUILayout.Space(10);

        // --- 3. 生態關係 ---
        EditorGUILayout.BeginVertical("box");
        DrawIntList(species.preyIDList, "Prey IDs");
        DrawIntList(species.predatorIDList, "Predator IDs");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // --- 4. 行為清單 ---
        DrawEnumList(species.foodTypes, "Food Types");
        DrawEnumList(species.actionList, "Available Actions");

        EditorGUILayout.Space(10);

        // --- 5. 冷卻設定 ---
        DrawCDDictionary();

        // 如果介面有任何變動
        if (EditorGUI.EndChangeCheck())
        {
            // 紀錄變更以便 Undo
            Undo.RecordObject(species, "Modify Species Attributes");

            // 把修改後的 struct 存回去
            //species.attributes = attr;----------------------------------------------先關掉

            // 強制 Unity 儲存這個資產檔
            EditorUtility.SetDirty(species);

            // 如果是在 Prefab 或特定的數據結構，這行能確保存檔
            //AssetDatabase.SaveAssets();
        }

        // 將序列化物件的更改套用（雖然你用 target 轉型修改，但這行是好習慣）
        serializedObject.ApplyModifiedProperties();
    }
    public void DrawBaseAttributes()
    {
        EditorGUILayout.Space(10);

        // --- 分組設計：核心屬性 ---
        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.LabelField("Core Statistics", EditorStyles.miniBoldLabel);
        species.creatureBase = (CreatureBase)EditorGUILayout.EnumPopup("Creature Base Type", species.creatureBase);
        species.speciesID = EditorGUILayout.IntField("Species ID", species.speciesID);
        species.baseSize = EditorGUILayout.Slider("Size", species.baseSize, 0.1f, 1f);
        species.baseSpeed = EditorGUILayout.Slider("Speed", species.baseSpeed, 0f, 20f);
        species.baseMaxHealth = EditorGUILayout.Slider("Base Health", species.baseMaxHealth, 0f, 100f);
        species.baseReproductionRate = EditorGUILayout.Slider("Reproduction Rate", species.baseReproductionRate, 0f, 1f);
        species.baseAttackPower = EditorGUILayout.Slider("Attack Power", species.baseAttackPower, 0f, 100f);
        species.baseLifespan = EditorGUILayout.Slider("Lifespan", species.baseLifespan, 0, 10000);
        species.variation = EditorGUILayout.Slider("Variation", species.variation, 0f, 1f);
        species.basePerceptionRange = EditorGUILayout.Slider("Perception Range", species.basePerceptionRange, 0, 100);

        // 睡眠區間 (MinMaxSlider 需要 float 變數)
        float head = species.baseSleepingHead;
        float tail = species.baseSleepingTail;
        EditorGUILayout.MinMaxSlider("Sleep Interval", ref head, ref tail, 0f, 100f);
        species.baseSleepingHead = (int)head;
        species.baseSleepingTail = (int)tail;

        EditorGUILayout.EndVertical();

    }
    private void DrawIntList(List<int> list, string label)
    {
        // 繪製標題
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        // 使用 HelpBox 風格的容器讓視覺有區隔
        EditorGUILayout.BeginVertical("helpbox");

        if (list == null || list.Count == 0)
        {
            EditorGUILayout.LabelField("List is empty.", EditorStyles.miniLabel);
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // 顯示索引 (i+1) 讓開發者好讀
                EditorGUILayout.LabelField($"[{i}]", GUILayout.Width(30));

                // 繪製整數輸入框
                int newValue = EditorGUILayout.IntField(list[i]);

                // 檢查數值是否改變 (用於支援 Undo)
                if (newValue != list[i])
                {
                    Undo.RecordObject(target, $"Modify {label}");
                    list[i] = newValue;
                }

                // 刪除按鈕
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f); // 淺紅色
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    Undo.RecordObject(target, $"Remove from {label}");
                    list.RemoveAt(i);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                    break; // 刪除後跳出循環，避免索引溢出
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.EndHorizontal();
            }
        }

        // 新增按鈕
        if (GUILayout.Button($"+ Add New {label} Entry"))
        {
            Undo.RecordObject(target, $"Add to {label}");
            list.Add(0); // 預設新增 0
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(5);
    }
    private void DrawEnumList<T>(List<T> list, string label) where T : System.Enum
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            list[i] = (T)EditorGUILayout.EnumPopup(list[i]);
            if (GUILayout.Button("-", GUILayout.Width(25))) { list.RemoveAt(i); break; }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button($"+ Add {label}")) list.Add(default(T));
        EditorGUILayout.EndVertical();
    }
    

    private void DrawCDDictionary()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Action Max CD Settings", EditorStyles.boldLabel);

        // 確保字典已初始化
        if (species.actionMaxCD == null)
            species.actionMaxCD = new Dictionary<ActionType, int>();

        var dict = species.actionMaxCD;

        // 1. 繪製現有的條目
        List<ActionType> keys = new List<ActionType>(dict.Keys);
        foreach (var key in keys)
        {
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField(key.ToString(), GUILayout.Width(120));

            int newValue = EditorGUILayout.IntField(dict[key]);
            if (newValue != dict[key])
            {
                Undo.RecordObject(species, "Change Action CD"); // 支援 Ctrl+Z
                dict[key] = newValue;
            }

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                Undo.RecordObject(species, "Remove Action CD");
                dict.Remove(key);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        // 2. 新增條目的介面
        EditorGUILayout.BeginHorizontal();
        selectedActionToCD = (ActionType)EditorGUILayout.EnumPopup(selectedActionToCD);
        if (GUILayout.Button("Add New CD Rule"))
        {
            if (!dict.ContainsKey(selectedActionToCD))
            {
                Undo.RecordObject(species, "Add Action CD");
                dict.Add(selectedActionToCD, 0);
            }
            else
            {
                Debug.LogWarning("This Action already has a CD rule!");
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}