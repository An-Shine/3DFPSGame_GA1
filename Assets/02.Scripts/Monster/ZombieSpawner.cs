using UnityEngine;
using Redcode.Pools;

public class ZombieSpawner : MonoBehaviour
{
    [Header("풀")]
    [SerializeField] private Zombie _zombie;
    [SerializeField] private int _poolSize = 10;

    [Header("스폰")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _spawnInterval = 3f;

    private IPool<Zombie> _pool;
    private float _spawnTimer;

    private void Awake()
    {
        _pool = Pool.Create(_zombie, _poolSize, transform).NonLazy();
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        _spawnTimer += Time.deltaTime;
        if (_spawnTimer < _spawnInterval)
        {
            return;
        }
        _spawnTimer = 0f;
        SpawnZombie();
    }

    private void SpawnZombie()
    {
        Zombie zombie = _pool.Get();
        if (zombie == null)
        {
            return;
        }

        Transform point = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        zombie.Spawn(_pool, point.position, point.rotation);
    }

}
