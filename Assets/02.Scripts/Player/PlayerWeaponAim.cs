using UnityEngine;

// 총구가 회전한 다음 레이저의 출발점을 갱신하도록 먼저 실행한다.
[DefaultExecutionOrder(-10)]
public class PlayerWeaponAim : MonoBehaviour
{
    [SerializeField] private Transform _aimCamera;
    [SerializeField] private Transform[] _weapons;

    private Quaternion[] _rotationOffsets;

    private void Awake()
    {
        _rotationOffsets = new Quaternion[_weapons.Length];

        for (int i = 0; i < _weapons.Length; i++)
        {
            // FBX의 기본 회전 방향을 보존한다.
            _rotationOffsets[i] = Quaternion.Inverse(transform.rotation) * _weapons[i].rotation;
        }
    }

    private void LateUpdate()
    {
        // 카메라의 상하 각도와 플레이어의 좌우 방향을 조합한다.
        float pitch = Mathf.DeltaAngle(0f, _aimCamera.eulerAngles.x);
        Quaternion aimRotation = Quaternion.Euler(pitch, transform.eulerAngles.y, 0f);

        for (int i = 0; i < _weapons.Length; i++)
        {
            _weapons[i].rotation = aimRotation * _rotationOffsets[i];
        }
    }
}
