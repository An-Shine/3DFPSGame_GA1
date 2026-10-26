using UnityEngine;

// 미니맵 카메라 : 플레이어 머리 위에서 아래를 내려다보며 따라다닌다
[RequireComponent(typeof(Camera))]
public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _height = 8f;

    [Header("줌")]
    [SerializeField] private float _zoomStep = 2f;
    [SerializeField] private float _minSize = 4f;
    [SerializeField] private float _maxSize = 16f;

    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        // 시작 크기가 허용 범위를 벗어나 있으면 보정한다
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, _minSize, _maxSize);
    }

    private void LateUpdate()
    {
        // 1. 대상(플레이어)의 머리 위로 위치를 옮긴다
        transform.position = _target.position + (Vector3.up * _height);

        transform.eulerAngles = new Vector3(90f, _target.eulerAngles.y, 0f);
    }

    public void ZoomIn()
    {
        float size = _camera.orthographicSize - _zoomStep;
        _camera.orthographicSize = Mathf.Max(size, _minSize);
    }

    public void ZoomOut()
    {
        float size = _camera.orthographicSize + _zoomStep;
        _camera.orthographicSize = Mathf.Min(size, _maxSize);
    }
}
