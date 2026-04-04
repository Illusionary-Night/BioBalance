using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    // 全域單例，供其他系統快速存取 InputManager
    public static InputManager Instance { get; private set; }

    // Unity Input System 自動產生的輸入封裝
    private InGameControl inGameControl;

    // Input Events
    // ===================================================================
    // Player Control Events
    public event Action<Vector2> OnMove;
    public event Action OnMoveStarted;
    public event Action OnMoveCanceled;
    public event Action OnInteractPerformed;
    public event Action OnInteractCanceled;

    // General Control Events
    public event Action OnCtrlPerformed;
    public event Action OnCtrlCanceled;
    public event Action OnPausePerformed;
    public event Action OnPauseCanceled;

    //Window Control Events
    public event Action<Vector2> OnZoom;
    public event Action OnZoomStarted;
    public event Action OnZoomCanceled;
    public event Action<Vector2> OnPanCamera;
    public event Action OnPanCameraStarted;
    public event Action OnPanCameraCanceled;

    // ===================================================================

    // Input States（供外部查詢目前輸入狀態）
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsInteracting { get; private set; }

    // 只在 Ctrl 按住時允許 PanCamera
    private bool isCtrlPressed;

    // 初始化單例與輸入綁定
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        inGameControl = new InGameControl();
        RegisterInputActions();
    }

    // 物件啟用時啟用整份 InputActionAsset
    void OnEnable()
    {
        inGameControl?.Enable();
    }

    // 物件停用時停用整份 InputActionAsset
    void OnDisable()
    {
        inGameControl?.Disable();
    }

    // 物件銷毀前解除事件綁定，避免殘留回呼
    void OnDestroy()
    {
        UnregisterInputActions();
    }

    // 註冊所有需要的 InputAction 回呼
    private void RegisterInputActions()
    {
        // Movement
        inGameControl.Player.Move.performed += OnMovePerformed;
        inGameControl.Player.Move.started += OnMoveStartedCallback;
        inGameControl.Player.Move.canceled += OnMoveCanceledCallback;

        // Interact
        inGameControl.Player.Interact.performed += OnInteractPerformedCallback;
        inGameControl.Player.Interact.canceled += OnInteractCanceledCallback;

        // General
        inGameControl.General.Modifier_Ctrl.performed += OnCtrlPerformedCallback;
        inGameControl.General.Modifier_Ctrl.canceled += OnCtrlCanceledCallback;
        inGameControl.General.Pause.performed += OnPausePerformedCallback;
        inGameControl.General.Pause.canceled += OnPauseCanceledCallback;

        // PanCamera
        inGameControl.WindowControl.PanCamera.performed += OnPanCameraPerformed;
        inGameControl.WindowControl.PanCamera.started += OnPanCameraStartedCallback;
        inGameControl.WindowControl.PanCamera.canceled += OnPanCameraCanceledCallback;

        // Zoom
        inGameControl.WindowControl.Zoom.started += OnZoomStartedCallback;
        inGameControl.WindowControl.Zoom.performed += OnZoomPerformedCallback;
        inGameControl.WindowControl.Zoom.canceled += OnZoomCanceledCallback;
    }

    // 解除所有 InputAction 回呼，與 RegisterInputActions 對應
    private void UnregisterInputActions()
    {
        if (inGameControl == null) return;

        inGameControl.Player.Move.performed -= OnMovePerformed;
        inGameControl.Player.Move.started -= OnMoveStartedCallback;
        inGameControl.Player.Move.canceled -= OnMoveCanceledCallback;

        inGameControl.Player.Interact.performed -= OnInteractPerformedCallback;
        inGameControl.Player.Interact.canceled -= OnInteractCanceledCallback;

        inGameControl.General.Modifier_Ctrl.performed -= OnCtrlPerformedCallback;
        inGameControl.General.Modifier_Ctrl.canceled -= OnCtrlCanceledCallback;
        inGameControl.General.Pause.performed -= OnPausePerformedCallback;
        inGameControl.General.Pause.canceled -= OnPauseCanceledCallback;

        inGameControl.WindowControl.PanCamera.performed -= OnPanCameraPerformed;
        inGameControl.WindowControl.PanCamera.started -= OnPanCameraStartedCallback;
        inGameControl.WindowControl.PanCamera.canceled -= OnPanCameraCanceledCallback;

        inGameControl.WindowControl.Zoom.started -= OnZoomStartedCallback;
        inGameControl.WindowControl.Zoom.performed -= OnZoomPerformedCallback;
        inGameControl.WindowControl.Zoom.canceled -= OnZoomCanceledCallback;
    }

    // 移動輸入持續觸發時更新狀態並派發事件
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
        OnMove?.Invoke(MoveInput);
    }

    // 開始移動時派發事件
    private void OnMoveStartedCallback(InputAction.CallbackContext context)
    {
        OnMoveStarted?.Invoke();
    }

    // 停止移動時重置狀態並派發事件
    private void OnMoveCanceledCallback(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
        OnMoveCanceled?.Invoke();
    }

    // 互動按下時更新狀態並派發事件
    private void OnInteractPerformedCallback(InputAction.CallbackContext context)
    {
        IsInteracting = true;
        OnInteractPerformed?.Invoke();
    }

    // 互動放開時更新狀態並派發事件
    private void OnInteractCanceledCallback(InputAction.CallbackContext context)
    {
        IsInteracting = false;
        OnInteractCanceled?.Invoke();
    }

    // Ctrl 按下時記錄狀態並派發事件
    private void OnCtrlPerformedCallback(InputAction.CallbackContext context)
    {
        // 記錄 Ctrl 狀態，供 PanCamera 觸發條件判斷
        isCtrlPressed = true;
        OnCtrlPerformed?.Invoke();
    }

    // Ctrl 放開時清理 Pan 狀態並派發事件
    private void OnCtrlCanceledCallback(InputAction.CallbackContext context)
    {
        // 放開 Ctrl 時立即停止相機平移輸入
        isCtrlPressed = false;
        if (LookInput != Vector2.zero)
        {
            LookInput = Vector2.zero;
            OnPanCameraCanceled?.Invoke();
        }

        OnCtrlCanceled?.Invoke();
    }

    // Pause 按下時派發事件
    private void OnPausePerformedCallback(InputAction.CallbackContext context)
    {
        OnPausePerformed?.Invoke();
    }

    // Pause 放開時派發事件
    private void OnPauseCanceledCallback(InputAction.CallbackContext context)
    {
        OnPauseCanceled?.Invoke();
    }

    // 相機平移開始：僅在 Ctrl 按住時才有效
    private void OnPanCameraStartedCallback(InputAction.CallbackContext context)
    {
        // 需 Ctrl + PanCamera 才觸發
        if (!isCtrlPressed) return;
        OnPanCameraStarted?.Invoke();
    }

    // 相機平移進行中：僅在 Ctrl 按住時才更新輸入
    private void OnPanCameraPerformed(InputAction.CallbackContext context)
    {
        // 需 Ctrl + PanCamera 才觸發
        if (!isCtrlPressed) return;

        LookInput = context.ReadValue<Vector2>();
        OnPanCamera?.Invoke(LookInput);
    }

    // 相機平移結束時重置狀態並派發事件
    private void OnPanCameraCanceledCallback(InputAction.CallbackContext context)
    {
        if (!isCtrlPressed && LookInput == Vector2.zero) return;

        LookInput = Vector2.zero;
        OnPanCameraCanceled?.Invoke();
    }

    // 縮放開始時派發事件
    private void OnZoomStartedCallback(InputAction.CallbackContext context)
    {
        OnZoomStarted?.Invoke();
    }

    // 縮放進行中讀取 Vector2（例如滑鼠滾輪/手把軸）並派發事件
    private void OnZoomPerformedCallback(InputAction.CallbackContext context)
    {
        var zoomInput = context.ReadValue<Vector2>();
        OnZoom?.Invoke(zoomInput);
    }

    // 縮放取消時派發事件
    private void OnZoomCanceledCallback(InputAction.CallbackContext context)
    {
        OnZoomCanceled?.Invoke();
    }

    // Public Methods（外部切換 Action Map / 查詢按鍵狀態）

    // 啟用 Player Action Map
    public void EnablePlayerInput()
    {
        inGameControl.Player.Enable();
    }

    // 停用 Player Action Map
    public void DisablePlayerInput()
    {
        inGameControl.Player.Disable();
    }

    // 啟用 General Action Map（保留原命名相容）
    public void EnableUIInput()
    {
        inGameControl.General.Enable();
    }

    // 停用 General Action Map（保留原命名相容）
    public void DisableUIInput()
    {
        inGameControl.General.Disable();
    }

    // 切到 WindowControl，並關閉 General
    public void SwitchToWindowActionMap()
    {
        inGameControl.General.Disable();
        inGameControl.WindowControl.Enable();
    }

    // 切到 General，並關閉 WindowControl
    public void SwitchToUIActionMap()
    {
        inGameControl.WindowControl.Disable();
        inGameControl.General.Enable();
    }

    // 檢查指定 InputAction 是否目前處於按下狀態
    public bool IsActionPressed(InputAction action)
    {
        return action.IsPressed();
    }

    // 檢查指定 InputAction 是否在本幀觸發
    public bool WasActionPerformedThisFrame(InputAction action)
    {
        return action.WasPerformedThisFrame();
    }
}
