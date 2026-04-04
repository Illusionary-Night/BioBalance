using UnityEngine;
using UnityEngine.InputSystem.Editor;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance { get; private set; }
    public static InputManager inputManager;
    public static InGameManager inGameManager;

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            LogManager.LogWarning("[MainManager] 已存在 MainManager 實例，銷毀重複的 MainManager。");
            return;
        }

        Initialize();
    }
    void Start()
    {
        // 開始遊戲
        GameStart();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Initialize()
    {
        NewManager<InputManager>(ref inputManager);
    }

    private void GameStart()
    {
        NewManager<InGameManager>(ref inGameManager);
    }

    private static void NewManager<T> (ref T attr)
        where T : class, new()
    {
        if (attr == null) { attr = new T(); }
        else              { LogManager.LogWarning($"[{typeof(T).Name}] Exist, "); }
    }
}
