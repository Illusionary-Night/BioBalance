using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Species", menuName = "BioBalance/Species")]
public class _Species : ScriptableObject, ISerializationCallbackReceiver
{
    public CreatureAttributes attributes;

    // 用來給 Unity 存檔用的「影子變數」
    [SerializeField, HideInInspector] private List<ActionType> _cdKeys = new List<ActionType>();
    [SerializeField, HideInInspector] private List<int> _cdValues = new List<int>();

    // 存檔前執行：將 Dictionary 拆解到 List
    public void OnBeforeSerialize()
    {
        _cdKeys.Clear();
        _cdValues.Clear();

        if (attributes.action_max_CD == null) return;

        foreach (var kvp in attributes.action_max_CD)
        {
            _cdKeys.Add(kvp.Key);
            _cdValues.Add(kvp.Value);
        }
    }

    // 讀取後執行：將 List 重新組合成 Dictionary
    public void OnAfterDeserialize()
    {
        attributes.action_max_CD = new Dictionary<ActionType, int>();

        for (int i = 0; i != Mathf.Min(_cdKeys.Count, _cdValues.Count); i++)
        {
            attributes.action_max_CD.Add(_cdKeys[i], _cdValues[i]);
        }
    }
}