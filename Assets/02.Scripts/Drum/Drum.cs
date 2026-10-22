using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Drum : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;

    [Header("스탯")]
    [SerializeField] private float _health = 30f;
    [SerializeField] private float _launchPower = 10f;
    [SerializeField] private float _spinPower = 5f;
    [SerializeField] private float _destroyDelay = 3f;
    [SerializeField] private float _explosionRange = 4.5f;
    [SerializeField] private float _damage = 100f;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Damage(float damage)
    {
        if (_health <= 0) return;

        _health -= damage;

        if (_health <= 0) StartCoroutine(Explode_Coroutine());
    }

    private IEnumerator Explode_Coroutine()
    {
        // 1. 폭발 이펙트
        ParticleSystem effect = Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);
        effect.Play();

        // 2. 위로 튀어오르면서 빙글빙글 돈다
        _rigidbody.AddForce(Vector3.up * _launchPower, ForceMode.Impulse);
        _rigidbody.AddTorque(Random.insideUnitSphere * _spinPower, ForceMode.Impulse);

        // 3. 주위에 폭발데미지를 준다
        ApplyDamage();

        // 4. 사라진다
        yield return new WaitForSeconds(_destroyDelay);
        Destroy(gameObject);
    }

    private void ApplyDamage()
    {
        // 0. 주위에 있는 드럼통을 모두 찾는다
        Collider[] colliders =
            Physics.OverlapSphere(transform.position, _explosionRange, LayerMask.GetMask("Drum", "Zombie"));

        // 1. 드럼통들에게 데미지를준다
        foreach (Collider col in colliders)
        {
            col.GetComponent<Drum>()?.Damage(_damage);
            col.GetComponent<Zombie>()?.TakeDamage(_damage);
        }
    }
}