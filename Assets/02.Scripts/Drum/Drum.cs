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

        // 2. 위로 튀어오르면서 빙글빙글 돈다
        _rigidbody.AddForce(Vector3.up * _launchPower, ForceMode.Impulse);
        _rigidbody.AddTorque(Random.insideUnitSphere * _spinPower, ForceMode.Impulse);

        // 3. 사라진다
        yield return new WaitForSeconds(_destroyDelay);
        Destroy(gameObject);
    }
}