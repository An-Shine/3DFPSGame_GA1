using UnityEngine;

public class Katana : MonoBehaviour
{
    [SerializeField] private float _damage = 25f;  // 대미지

    private Player _player;

    private void Awake()
    {
        _player = GetComponentInParent<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 내 몸(Player의 Character Controller)에 닿은 것은 무시
        if (other.transform == _player.transform) return;

        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            DamageInfo damageInfo = new DamageInfo();
            damageInfo.Value = _damage;
            Vector3 hitPoint = other.ClosestPoint(_player.transform.position);  // 대상에서 플레이어에 가장 가까운 점
            damageInfo.HitPoint = hitPoint;
            damageInfo.HitNormal = (_player.transform.position - hitPoint).normalized;  // 맞은 자리에서 플레이어 쪽

            damageable.TakeDamage(damageInfo);
        }
    }
}