using UnityEngine;
using System;

public class Player : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private float _maxHealth = 100f;
    private float _health;

    public float MaxHealth => _maxHealth;
    public float Health => _health;

    private WeaponMode _weaponMode = WeaponMode.Normal;
    public WeaponMode WeaponMode => _weaponMode;

    public event Action OnDamaged;  // Action 자료형은 함수들을 저장할 수 있음, event 를 붙이면 외부에서 수정 불가능
    public event System.Action<WeaponMode> OnModeChanged;

    private Animator _animator;
    private int _injuredLayerIndex;

    private void Start()
    {
        _animator = GetComponentInChildren<Animator>();
        _injuredLayerIndex = _animator.GetLayerIndex("Injured");
        _health = _maxHealth;
    }

    private void Update()
    {

        if (GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        // 숫자 키 1은 일반 모드, 2는 저격 모드
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _weaponMode = WeaponMode.Normal;
            OnModeChanged?.Invoke(WeaponMode.Normal);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _weaponMode = WeaponMode.Sniper;
            OnModeChanged?.Invoke(WeaponMode.Sniper);
        }
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        _health -= damageInfo.Value;

        // 체력에 따라 Injured 레이어의 가중치를 바꿔준다
        float healthPercent = _health / _maxHealth;     // 0 ~ 1 사이
        float injured = Mathf.InverseLerp(0.5f, 0f, healthPercent);
        _animator.SetLayerWeight(_injuredLayerIndex, injured);


        OnDamaged?.Invoke();

        Debug.Log($"플레이어가 맞았다! 데미지 {damageInfo.Value}, 남은 체력 : {_health}");
        if (_health <= 0)
        {
            Debug.Log("플레이어가 쓰러졌다");
            GameManager.Instance.GameOver();
        }
    }
}
