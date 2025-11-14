using UnityEngine;

public class TestRunner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public CreatureAttributes CreatureAttributes;
    [SerializeField] public GameObject CreatureObject;
    private Creature creature;
    void Start()
    {
        ActionSystem.DebugRegisteredActions();
        CreatureObject.GetComponent<Creature>().Initialize(CreatureAttributes,CreatureObject);
        creature = CreatureObject.GetComponent<Creature>();
    }

    // Update is called once per frame
    void Update()
    {
        // 滑鼠左鍵：設定目的地
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int dest = Vector2Int.RoundToInt(new Vector2(worldPos.x, worldPos.y));
            creature.MoveTo(dest);
        }

        // 按空白鍵：重新導航
        if (Input.GetKeyDown(KeyCode.Space))
        {
            creature.ForceNavigate();
        }
    }

    // 物理步驟
    void FixedUpdate()
    {
        if (creature != null)
            creature.OnTick(); // <- 呼叫 Movement.FixedTick() 內部
    }
}
