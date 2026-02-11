using TMPro;
using UnityEngine;

public class TimeClock : MonoBehaviour
{
    public TextMeshProUGUI textUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        textUI.text = $"Day {Manager.Instance.TickManager.CurrentDay} {Manager.Instance.TickManager.CurrentHour}:00";
    }
}
