using UnityEngine;

public class TestRunner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public bool ManualControl = true;
    [SerializeField] public _Species goat;
    [SerializeField] public GameObject CreatureObject;
    private Creature creature;
    void Start()
    {
        //ActionSystem.DebugRegisteredActions();
        CreatureObject.GetComponent<Creature>().Initialize(goat.attributes, CreatureObject);
        creature = CreatureObject.GetComponent<Creature>();
    }

    // Update is called once per frame
    void Update()
    {
        // �ƹ�����G�]�w�ت��a
        if (Input.GetMouseButtonDown(0) && ManualControl)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int dest = Vector2Int.RoundToInt(new Vector2(worldPos.x, worldPos.y));
            creature.MoveTo(dest);
        }

        // ���ť���G���s�ɯ�
        if (Input.GetKeyDown(KeyCode.Space) && ManualControl)
        {
            creature.ForceNavigate();
        }
    }
}
