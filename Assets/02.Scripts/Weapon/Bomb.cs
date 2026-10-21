using UnityEngine;
using Redcode.Pools;

public class Bomb : MonoBehaviour
{
    [SerializeField] private ParticleSystem _explosionEffectPrefab;

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
        if (_hasExploded)
        {
            return;
        }
        _hasExploded = true;

        ParticleSystem effect = Instantiate(_explosionEffectPrefab);
        effect.transform.position = transform.position;
        effect.Play();

        _pool.Take(this);
    }
}
