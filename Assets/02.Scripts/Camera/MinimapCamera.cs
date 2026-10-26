using UnityEngine;

// 미니맵 카메라 : 플레이어 머리 위에서 아래를 내려다보며 따라다닌다
public class MinimapCamera : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _height = 8f;

    private Camera _camera;

    private void Start()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            _camera.orthographicSize -= 1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _camera.orthographicSize += 1;
        }
    }
    private void LateUpdate()
    {
        // 1. 대상(플레이어)의 머리 위로 위치를 옮긴다
        transform.position = _target.position + (Vector3.up * _height);

        transform.eulerAngles = new Vector3(90f, _target.eulerAngles.y, 0f);
    }
}
