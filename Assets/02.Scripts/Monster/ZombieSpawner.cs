using UnityEngine;
using Redcode.Pools;

public class ZombieSpawner : MonoBehaviour
{
    [Header("풀")]
    [SerializeField] private PoolManager _poolManager;

    [Header("스폰")]
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _spawnInterval = 3f;
    [SerializeField] private int _maxSpawnCount = 10;   // 총 스폰 한도 (풀 크기와 무관)

    private IPool<Zombie> _pool;
    private float _spawnTimer;
    private int _totalSpawnCount;   // 지금까지 스폰한 누적 수
    private int _aliveCount;        // 현재 살아있는 좀비 수

    public int AliveCount => _aliveCount;
    public int MaxSpawnCount => _maxSpawnCount;

    public event System.Action OnCountChanged;

    private void Start()
    {
        // PoolManager가 Awake에서 풀을 만들므로 Start에서 가져옴
        _pool = _poolManager.GetPool<Zombie>("Zombie");
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        if (_totalSpawnCount >= _maxSpawnCount)
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
        if (!zombie.Spawn(_pool, point.position, point.rotation))
        {
            // NavMesh에 놓을 수 없는 객체는 반환하고 생성 수에 포함하지 않음
            _pool.Take(zombie);
            return;
        }
        zombie.OnDeath += HandleZombieDeath;

        _totalSpawnCount++;
        _aliveCount++;
        OnCountChanged?.Invoke();
    }

    private void HandleZombieDeath(Zombie zombie)
    {
        // 좀비는 풀링으로 재사용되므로 구독 중복을 막기 위해 해제
        zombie.OnDeath -= HandleZombieDeath;

        _aliveCount--;
        OnCountChanged?.Invoke();
    }
}
