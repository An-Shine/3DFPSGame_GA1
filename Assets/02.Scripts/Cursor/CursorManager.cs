using UnityEngine;
using UnityEngine.EventSystems;

// 마우스 커서를 화면 가운데에 잠그고 숨김
public class CursorManager : SingletonBehaviour<CursorManager>
{
    private CameraFollow _cameraFollow;
    public bool CanAim => GameManager.Instance.State == GameState.Playing &&
                          !Input.GetKey(KeyCode.LeftAlt) &&
                          (_cameraFollow.Mode == CameraMode.QuarterView
                              ? EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()
                              : Cursor.lockState == CursorLockMode.Locked);
    protected override void Awake()
    {
        base.Awake();
        _cameraFollow = Camera.main.GetComponent<CameraFollow>();
        _cameraFollow.OnCameraModeChanged += OnCameraModeChanged;
        LockCursor();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            UnlockCursor();
        }

        else if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            LockCursor();
        }
    }

    public void LockCursor()
    {
        // 쿼터뷰에서는 중앙에 고정하지 않고 Crosshair를 커서로 사용한다.
        Cursor.lockState = _cameraFollow.Mode == CameraMode.QuarterView
            ? CursorLockMode.Confined
            : CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnCameraModeChanged(CameraMode mode)
    {
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            UnlockCursor();
        }
        else
        {
            LockCursor();
        }
    }

    private void OnDestroy()
    {
        if (_cameraFollow != null)
        {
            _cameraFollow.OnCameraModeChanged -= OnCameraModeChanged;
        }
    }
}
