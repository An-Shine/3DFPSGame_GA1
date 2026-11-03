using UnityEngine;

public class PlayerRotate : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 200f;

    private float _accumulationX = 0f;  // 좌우 회전 각도
    private CameraFollow _cameraFollow;

    private void Awake()
    {
        _cameraFollow = Camera.main.GetComponent<CameraFollow>();
    }

    private void Update()
    {
        if (!CursorManager.Instance.CanAim)
        {
            return;
        }

        if (_cameraFollow.Mode == CameraMode.QuarterView)
        {
            // 몸은 기울이지 않고 마우스가 가리키는 수평 방향을 바라본다.
            transform.rotation = Quaternion.LookRotation(_cameraFollow.GetAimRay().direction);
            _accumulationX = transform.eulerAngles.y;
            return;
        }

        float mouseX = Input.GetAxis("Mouse X");
        _accumulationX += mouseX * _rotationSpeed * Time.deltaTime;
        transform.eulerAngles = new Vector3(0f, _accumulationX, 0f);
    }
}
