using UnityEngine;
using TMPro;
using Redcode.Pools;

public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePosition;
    [SerializeField] private PoolManager _poolManager;
    [SerializeField] private float _throwPower = 15f;
    [SerializeField] private float _maxThrowPower = 30f;
    [SerializeField] private int _bombCount = 10;
    [SerializeField] private TMP_Text _bombCountUI;

    [Min(0.01f)][SerializeField] private float _maxChargeTime = 2f;

    private float _chargeTime;
    private bool _isCharging = false;
    private IPool<Bomb> _bombPool;

    private void Awake()
    {
        ShowBombCount();
    }

    private void Start()
    {
        _bombPool = _poolManager.GetPool<Bomb>("Bomb");
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        if (Input.GetButtonDown("Fire2") && _bombCount > 0)
        {
            _chargeTime = 0f;
            _isCharging = true;
        }

        if (_isCharging && Input.GetButton("Fire2"))
        {
            _chargeTime = Mathf.Min(_chargeTime + Time.deltaTime, _maxChargeTime);
        }

        if (_isCharging && Input.GetButtonUp("Fire2"))
        {
            _isCharging = false;
            Shoot();
            _chargeTime = 0f;
        }
    }

    private void Shoot()
    {
        float power = _throwPower + (_maxThrowPower - _throwPower) * (_chargeTime / _maxChargeTime);
        Bomb bomb = _bombPool.Get();
        if (bomb == null)
        {
            return;
        }

        bomb.Launch(_bombPool, _firePosition.position, Camera.main.transform.forward, power);
        _bombCount--;
        ShowBombCount();
    }

    private void ShowBombCount()
    {
        _bombCountUI.text = $"{_bombCount}";
    }
}