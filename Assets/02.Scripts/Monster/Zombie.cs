using System;
using System.Collections.Generic;
using Redcode.Pools;
using UnityEngine;

public enum ZombieState
{
    Idle,
    Trace,
    Comeback,
    Attack,
    Hit,
    Death,
    Patrol
}

[RequireComponent(typeof(CharacterController))]
public class Zombie : MonoBehaviour, IDamageable
{
    [Header("스탯")]
    [SerializeField] private float _detectionDistance = 8f;
    [SerializeField] private float _comebackDistance = 15f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _attackInterval = 1.2f;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _maxHealth = 30f;
    [SerializeField] private float _attackDamage = 10f;

    [Header("이펙트")]
    [SerializeField] private GameObject _bloodSprayPrefab;

    [Header("순찰")]
    [SerializeField, Min(0f)] private float _idleDuration = 2f;
    [SerializeField, Min(0.5f)] private float _patrolDistance = 4f;
    [SerializeField, Min(0f)] private float _patrolWaitDuration = 1f;
    [SerializeField, Min(0.05f)] private float _arrivalDistance = 0.2f;

    [Header("순찰 지점 검사")]
    [SerializeField] private LayerMask _groundMask = 1 << 8;
    [SerializeField] private LayerMask _obstacleMask = (1 << 9) | (1 << 10) | (1 << 12);

    private CharacterController _controller;
    private GameObject _player;
    private IPool<Zombie> _pool;
    private Dictionary<ZombieState, IZombieState> _states;
    private IZombieState _currentState;
    private ZombieHitState _hitState;
    private ZombieState _state = ZombieState.Idle;
    private Vector3 _startPosition;
    private Vector3 _moveDirection;
    private float _currentHealth;
    private bool _spawnInitialized;
    private readonly List<GameObject> _bloodSprays = new();

    public float Health => _currentHealth;
    public float MaxHealth => _maxHealth;
    public float DetectionDistance => _detectionDistance;
    public float ComebackDistance => _comebackDistance;
    public float AttackRange => _attackRange;
    public float AttackInterval => _attackInterval;
    public float AttackDamage => _attackDamage;
    public float IdleDuration => _idleDuration;
    public float PatrolDistance => _patrolDistance;
    public float PatrolWaitDuration => _patrolWaitDuration;
    public float ArrivalDistance => _arrivalDistance;
    public LayerMask GroundMask => _groundMask;
    public LayerMask ObstacleMask => _obstacleMask;
    public CharacterController Controller => _controller;
    public GameObject Player => _player;
    public GameObject BloodSprayPrefab => _bloodSprayPrefab;
    public Vector3 StartPosition => _startPosition;
    public IPool<Zombie> Pool => _pool;

    public event Action OnDamaged;
    public event Action<Zombie> OnDeath;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();

        // 상태 객체는 좀비마다 한 번 생성하고, 재진입 시 초기화해 재사용
        ZombiePatrolState patrolState = new ZombiePatrolState(this);
        _hitState = new ZombieHitState(this);
        _states = new Dictionary<ZombieState, IZombieState>
        {
            { ZombieState.Idle, new ZombieIdleState(this, patrolState) },
            { ZombieState.Patrol, patrolState },
            { ZombieState.Trace, new ZombieTraceState(this) },
            { ZombieState.Comeback, new ZombieComebackState(this) },
            { ZombieState.Attack, new ZombieAttackState(this) },
            { ZombieState.Hit, _hitState },
            { ZombieState.Death, new ZombieDeathState(this) }
        };
    }

    private void Start()
    {
        // 스포너가 먼저 초기화한 좀비를 Start에서 다시 초기화하지 않음
        if (!_spawnInitialized)
        {
            ResetForSpawn();
        }
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing)
        {
            return;
        }

        _moveDirection = Vector3.zero;

        // 사망 상태도 반환 대기 시간을 계산하므로 갱신
        _currentState?.UpdateState();

        if (_controller.enabled)
        {
            _controller.SimpleMove(_moveDirection * _moveSpeed);
        }
    }

    private void OnDisable()
    {
        _currentState?.ExitState();
        _currentState = null;
        StopMoving();
    }

    public void ChangeState(ZombieState nextState, bool restart = false)
    {
        // 연속 피격일 때만 같은 상태에 다시 진입해 경직 시간을 갱신
        if (_currentState != null && _state == nextState && !restart)
        {
            return;
        }

        if (_currentState != null && _state == ZombieState.Death)
        {
            return;
        }

        ZombieState previousState = _state;
        _currentState?.ExitState();
        StopMoving();

        _state = nextState;
        _currentState = _states[nextState];
        if (previousState != nextState)
        {
            Debug.Log($"{previousState} -> {nextState}");
        }

        _currentState.EnterState();
    }

    public void Spawn(IPool<Zombie> pool, Vector3 position, Quaternion rotation)
    {
        _pool = pool;
        _controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        ResetForSpawn();
    }

    public void ResetForSpawn()
    {
        // 같은 Idle로 재사용돼도 진입 처리와 타이머 초기화를 다시 실행
        _currentState?.ExitState();
        _currentState = null;
        ClearBloodSprays();

        _controller.enabled = true;
        _currentHealth = _maxHealth;
        _startPosition = transform.position;
        _player = GameObject.FindGameObjectWithTag("Player");
        _spawnInitialized = true;

        ChangeState(ZombieState.Idle);
        OnDamaged?.Invoke();
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (_state == ZombieState.Death)
        {
            return;
        }

        _currentHealth = Mathf.Max(0f, _currentHealth - Mathf.Max(0f, damageInfo.Value));
        if (_currentHealth > 0f)
        {
            _hitState.SetDamageInfo(damageInfo);
            ChangeState(ZombieState.Hit, true);
        }
        else
        {
            ChangeState(ZombieState.Death);
        }

        // 사망하는 타격도 체력바에 0을 전달
        OnDamaged?.Invoke();
    }

    public void MoveTo(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.forward = direction.normalized;
        float step = _moveSpeed * Time.deltaTime;
        float ratio = step > 0f ? Mathf.Clamp01(direction.magnitude / step) : 0f;
        _moveDirection = direction.normalized * ratio;
    }

    public void StopMoving()
    {
        _moveDirection = Vector3.zero;
    }

    public float GetPlayerDistance()
    {
        return _player == null
            ? float.PositiveInfinity
            : Vector3.Distance(transform.position, _player.transform.position);
    }

    public float GetHorizontalDistance(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        direction.y = 0f;
        return direction.magnitude;
    }

    public void RegisterBloodSpray(GameObject bloodSpray)
    {
        _bloodSprays.Add(bloodSpray);
    }

    public void ClearBloodSprays()
    {
        // 혈흔은 Hit 종료 뒤에도 남으므로 수명 관리는 Zombie가 담당
        foreach (GameObject spray in _bloodSprays)
        {
            if (spray != null)
            {
                Destroy(spray);
            }
        }

        _bloodSprays.Clear();
    }

    public void NotifyDeath()
    {
        // event는 선언한 클래스에서만 발생시킴. Death 진입 시 한 번 호출
        OnDeath?.Invoke(this);
    }
}
