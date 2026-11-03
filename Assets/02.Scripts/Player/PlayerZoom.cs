using UnityEngine;

public class PlayerZoom : MonoBehaviour
{
    [SerializeField] private GameObject _sniperZoom;
    [SerializeField] private float _normalPov = 60f;
    [SerializeField] private float _sniperPov = 15f;

    private Player _player;
    private Camera _camera;
    private CameraFollow _cameraFollow;

    private void Awake()
    {
        _player = GetComponent<Player>();
        _camera = Camera.main;
        _cameraFollow = _camera.GetComponent<CameraFollow>();
    }
    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        bool isZooming = CursorManager.Instance.CanAim && _cameraFollow.Mode == CameraMode.FirstPerson &&
                         _player.WeaponMode == WeaponMode.Sniper && Input.GetMouseButton(1);
        _sniperZoom.SetActive(isZooming);
        _camera.fieldOfView = isZooming ? _sniperPov : _normalPov;
    }
}
