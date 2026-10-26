using UnityEngine;
using System;

public class Player : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private float _maxHealth = 100f;
    private float _health;

    public float MaxHealth => _maxHealth;
    public float Health => _health;

    public event Action OnDamaged;  // Action 자료형은 함수들을 저장할 수 있다, event 를 붙이면 외부에서 수정 불가능

    private void Start()
    {
        _health = _maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        _health -= damageInfo.Value;

        OnDamaged?.Invoke();

        Debug.Log($"플레이어가 맞았다! 데미지 {damageInfo.Value}, 남은 체력 : {_health}");
        if (_health <= 0)
        {
            Debug.Log("플레이어가 쓰러졌다");
            GameManager.Instance.GameOver();
        }
    }
}
