using System;
using UnityEngine;

[DefaultExecutionOrder(-30)]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private CameraMode _mode = CameraMode.FirstPerson;

    [Header("시점별 위치")]
    [SerializeField] private Vector3 _thirdPersonOffset = new Vector3(0.6f, 0.3f, -2.5f);
    [SerializeField] private Vector3 _backViewOffset = new Vector3(0f, 1f, -4.5f);
    [SerializeField] private float _quarterViewDistance = 12f;
    [SerializeField] private Vector3 _quarterViewAngle = new Vector3(45f, 45f, 0f);

    public CameraMode Mode => _mode;
    public event Action<CameraMode> OnCameraModeChanged;

    private Camera _camera;
    private Transform _player;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if (_target == null)
        {
            Debug.LogWarning("[CameraFollow] 카메라 타겟이 없습니다");
        }

        _player = _target.GetComponentInParent<Player>().transform;
    }
    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            ChangeMode();
        }

        if (_mode == CameraMode.QuarterView)
        {
            // 회전과 사격이 마우스 위치를 계산하기 전에 쿼터뷰 카메라를 맞춘다.
            UpdateQuarterView();
        }
    }

    private void ChangeMode()
    {
        int count = Enum.GetValues(typeof(CameraMode)).Length;
        // 마지막 시점 다음에는 첫 시점으로 돌아간다.
        _mode = (CameraMode)(((int)_mode + 1) % count);
        OnCameraModeChanged?.Invoke(_mode);
        Debug.Log($"카메라 모드 변경: {_mode}");
    }

    private void LateUpdate()
    {
        switch (_mode)
        {
            case CameraMode.FirstPerson:
                // 기존 눈 위치로 이동한다.
                transform.position = _target.position;
                break;
            case CameraMode.ThirdPerson:
                // 시선 방향을 기준으로 어깨 옆과 뒤로 떨어진다.
                transform.position = _target.position + transform.rotation * _thirdPersonOffset;
                break;
            case CameraMode.BackView:
                // 시선 방향을 기준으로 캐릭터 바로 뒤에 배치한다.
                transform.position = _target.position + transform.rotation * _backViewOffset;
                break;
            case CameraMode.QuarterView:
                UpdateQuarterView();
                break;
        }
    }

    public Ray GetAimRay()
    {
        if (_mode == CameraMode.QuarterView)
        {
            Vector3 origin = GetQuarterAimOrigin();
            Plane aimPlane = new Plane(Vector3.up, origin);
            Ray mouseRay = _camera.ScreenPointToRay(Input.mousePosition);

            // 적의 높이는 무시하고 플레이어의 사격 높이에서 좌우 방향만 구한다.
            if (aimPlane.Raycast(mouseRay, out float distance))
            {
                Vector3 direction = mouseRay.GetPoint(distance) - origin;
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    return new Ray(origin, direction.normalized);
                }
            }

            return new Ray(origin, _player.forward);
        }

        return new Ray(transform.position, transform.forward);
    }

    private Vector3 GetQuarterAimOrigin()
    {
        // 눈 위치의 앞뒤 오프셋 때문에 몸이 돌 때 카메라도 흔들리는 것을 막는다.
        return new Vector3(_player.position.x, _target.position.y, _player.position.z);
    }

    private void UpdateQuarterView()
    {
        transform.rotation = Quaternion.Euler(_quarterViewAngle);
        transform.position = GetQuarterAimOrigin() - transform.forward * _quarterViewDistance;
    }
}
