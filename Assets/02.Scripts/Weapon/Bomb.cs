using UnityEngine;
using Redcode.Pools;

public class Bomb : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;
    [SerializeField] private float _damage = 100;
    [SerializeField] private float _explosionRange = 4.5f;

    private IPool<Bomb> _pool;
    private Rigidbody _rigidbody;
    private bool _hasExploded;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Launch(IPool<Bomb> pool, Vector3 position, Vector3 direction, float power)
    {
        _pool = pool;

        transform.SetPositionAndRotation(position, Quaternion.identity);
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _hasExploded = false;

        _rigidbody.AddForce(direction * power, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasExploded) return;
        _hasExploded = true;

        ParticleSystem effect = Instantiate(_explosionEffectPrefab);
        effect.transform.position = transform.position;
        effect.Play();
        ApplyDamage();
        _pool.Take(this);
    }

    private void ApplyDamage()
    {
        // 0. 주위에 있는 드럼통을 모두 찾음
        Collider[] colliders =
            Physics.OverlapSphere(transform.position, _explosionRange, LayerMask.GetMask("Drum", "Zombie"));

        // 1. 드럼통들에게 데미지를 줌
        foreach (Collider col in colliders)
        {
            DamageInfo damageInfo = new()
            {
                Value = _damage,
                HitPoint = transform.position,
                HitNormal = transform.forward,
            };
            col.GetComponent<IDamageable>()?.TakeDamage(damageInfo);
        }
    }
}