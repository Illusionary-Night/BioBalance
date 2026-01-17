using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

[CustomEditor(typeof(_Species))]
public class SpeciesEditor : Editor
{
    private ActionType selectedActionToCD; // 在類別層級宣告
    _Species species;
    public override void OnInspectorGUI()
    {
        species = (_Species)target;
        // 加上這行可以自動處理多數欄位的 Undo
        serializedObject.Update();

        // 使用暫存變數處理 struct
        var attr = species.attributes;

        // 檢測 GUI 是否有變動
        EditorGUI.BeginChangeCheck();

        // --- 1. 繪製 DNA 設計師標題 ---
        EditorGUILayout.LabelField("Species DNA Designer", EditorStyles.boldLabel);
        EditorGUILayout.Space(10);

        // --- 2. 基礎數值 ---
        DrawBaseAttributes(ref attr);

        EditorGUILayout.Space(10);

        // --- 3. 生態關係 ---
        EditorGUILayout.BeginVertical("box");
        DrawIntList(attr.prey_ID_list, "Prey IDs");
        DrawIntList(attr.predator_ID_list, "Predator IDs");
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // --- 4. 行為清單 ---
        DrawEnumList(attr.foodTypes, "Food Types");
        DrawEnumList(attr.action_list, "Available Actions");

        EditorGUILayout.Space(10);

        // --- 5. 冷卻設定 ---
        DrawCDDictionary();

        // 如果介面有任何變動
        if (EditorGUI.EndChangeCheck())
        {
            // 紀錄變更以便 Undo
            Undo.RecordObject(species, "Modify Species Attributes");

            // 把修改後的 struct 存回去
            species.attributes = attr;

            // 強制 Unity 儲存這個資產檔
            EditorUtility.SetDirty(species);

            // 如果是在 Prefab 或特定的數據結構，這行能確保存檔
            //AssetDatabase.SaveAssets();
        }

        // 將序列化物件的更改套用（雖然你用 target 轉型修改，但這行是好習慣）
        serializedObject.ApplyModifiedProperties();
    }
    public void DrawBaseAttributes(ref CreatureAttributes attr)
    {
        EditorGUILayout.Space(10);

        // --- 分組設計：核心屬性 ---
        EditorGUILayout.BeginVertical("helpbox");
        EditorGUILayout.LabelField("Core Statistics", EditorStyles.miniBoldLabel);
        attr.creautreBase = (CreatureBase)EditorGUILayout.EnumPopup("Creature Base Type", attr.creautreBase);
        attr.species_ID = EditorGUILayout.IntField("Species ID", attr.species_ID);
        attr.size = EditorGUILayout.Slider("Size", attr.size, 0.1f, 1f);
        attr.speed = EditorGUILayout.Slider("Speed", attr.speed, 0f, 20f);
        attr.base_health = EditorGUILayout.Slider("Base Health", attr.base_health, 0f, 100f);
        attr.reproduction_rate = EditorGUILayout.Slider("Reproduction Rate", attr.reproduction_rate, 0f, 1f);
        attr.attack_power = EditorGUILayout.Slider("Attack Power", attr.attack_power, 0f, 100f);
        attr.lifespan = EditorGUILayout.Slider("Lifespan", attr.lifespan, 0, 10000);
        attr.variation = EditorGUILayout.Slider("Variation", attr.variation, 0f, 1f);
        attr.perception_range = EditorGUILayout.Slider("Perception Range", attr.perception_range, 0, 100);

        // 睡眠區間 (MinMaxSlider 需要 float 變數)
        float head = attr.sleeping_head;
        float tail = attr.sleeping_tail;
        EditorGUILayout.MinMaxSlider("Sleep Interval", ref head, ref tail, 0f, 100f);
        attr.sleeping_head = (int)head;
        attr.sleeping_tail = (int)tail;

        EditorGUILayout.EndVertical();

        attr.Body = (BodyType)EditorGUILayout.EnumPopup("Body Type", attr.Body);

        // 將修改後的數值存回 ScriptableObject
        species.attributes = attr;
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
        if (species.attributes.action_max_CD == null)
            species.attributes.action_max_CD = new Dictionary<ActionType, int>();

        var dict = species.attributes.action_max_CD;

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