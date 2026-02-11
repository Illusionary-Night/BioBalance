using UnityEngine;

public class TestRunner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public bool ManualControl = true;
    [SerializeField] public Species goat;
    [SerializeField] public Species slime;
    [SerializeField] public Species icedragon;
    [SerializeField] public Species tiger;
    private Transform TestParent;

    [SerializeField] private Camera cam;
    private Material originalMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private GameObject currentSelected;

    private void Awake()
    {
        TestParent = new GameObject("!!!TestParent!!!").transform;
    }

    void Start()
    {
        SpawnCreature1();
        SpawnCreature2();
        SpawnCreature3();
        SpawnCreature4();
    }

    private void Update()
    {
        if (ManualControl)
        {
            DetectPlayerControl();
        }
    }

    // ======== Creature Template ===========
    private void SpawnCreature1(Vector3? pos = null)
    {
        // 使用物件池取得新生物
        if (pos.HasValue == false)
        {
            pos = new Vector3(250, 250, 0);
        }
        Creature creature = CreaturePool.GetCreature(goat, goat.ToCreatureAttributes(), (Vector3)pos, TestParent);
        creature.gameObject.name = creature.creatureBase + "_" + creature.UUID;
        Manager.Instance.RegisterCreature(creature);
    }

    private void SpawnCreature2(Vector3? pos = null)
    {
        // 使用物件池取得新生物
        if (pos.HasValue == false)
        {
            pos = new Vector3(260, 250, 0);
        }
        // 使用物件池取得新生物
        Creature creature = CreaturePool.GetCreature(slime, slime.ToCreatureAttributes(), (Vector3)pos, TestParent);
        creature.gameObject.name = creature.creatureBase + "_" + creature.UUID;
        Manager.Instance.RegisterCreature(creature);
    }
    private void SpawnCreature3(Vector3? pos = null)
    {
        // 使用物件池取得新生物
        if (pos.HasValue == false)
        {
            pos = new Vector3(270, 270, 0);
        }
        Creature creature = CreaturePool.GetCreature(icedragon, icedragon.ToCreatureAttributes(), (Vector3)pos, TestParent);
        creature.gameObject.name = creature.creatureBase + "_" + creature.UUID;
        Manager.Instance.RegisterCreature(creature);
    }
    private void SpawnCreature4(Vector3? pos = null)
    {
        // 使用物件池取得新生物
        if (pos.HasValue == false)
        {
            pos = new Vector3(250, 260, 0);
        }
        Creature creature = CreaturePool.GetCreature(tiger, tiger.ToCreatureAttributes(), (Vector3)pos, TestParent);
        creature.gameObject.name = creature.creatureBase + "_" + creature.UUID;
        Manager.Instance.RegisterCreature(creature);
    }

    // ======== God Mod ===========
    private void DetectPlayerControl()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;

        // 按下數字鍵 1 生成羊
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnCreature1(mousePos);
        }
        // 按下數字鍵 2 生成史萊姆
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnCreature2(mousePos);
        }
        // 按下數字鍵 3 生成Blue Ice Dragon
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnCreature3(mousePos);
        }
        // 按下數字區 1 生成草
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Manager.Instance.EnvEntityManager.SpawnEntity(EntityData.SpawnableEntityType.Grass, mousePos);
        }
        // 按下數字區 2 生成肉
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            Manager.Instance.EnvEntityManager.SpawnEntity(EntityData.SpawnableEntityType.Meat, mousePos);
        }
        // 按下數字區 3 生成腐肉
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            Manager.Instance.EnvEntityManager.SpawnEntity(EntityData.SpawnableEntityType.Carrion, mousePos);
        }
        // 按下等於鍵暫停/繼續遊戲
        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Manager.Instance.TickManager.SetPause();
        }
        // 按下 Z 鍵將遊戲速度設置為 60 TPS
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Manager.Instance.TickManager.SetTPS(60);
        }
        // 按下 X 鍵將遊戲速度設置為 30 TPS
        if (Input.GetKeyDown(KeyCode.X))
        {
            Manager.Instance.TickManager.SetTPS(30);
        }
        // 按下 C 鍵將遊戲速度設置為 15 TPS
        if (Input.GetKeyDown(KeyCode.C))
        {
            Manager.Instance.TickManager.SetTPS(15);
        }
        // 按下滑鼠左鍵高亮物件
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos2D = cam.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider != null)
            {
                SelectObject(hit.collider.gameObject);
            }
        }
    }

    void SelectObject(GameObject obj)
    {
        // Restore previous
        if (currentSelected != null)
        {
            currentSelected.GetComponent<Renderer>().material = originalMaterial;
        }

        currentSelected = obj;

        Renderer renderer = obj.GetComponent<Renderer>();
        originalMaterial = renderer.material;
        renderer.material = highlightMaterial;
    }
}
