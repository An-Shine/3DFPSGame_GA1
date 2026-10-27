using System;
using System.Collections.Generic;
using Redcode.Pools;
using UnityEngine;
using UnityEngine.AI;

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

[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
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
    [SerializeField, Min(0.1f)] private float _patrolMoveSpeed = 1.5f;

    [Header("NavMesh 위치 검사")]
    [SerializeField, Min(0.1f)] private float _navMeshSampleDistance = 2f;

    private GameObject _player;
    private IPool<Zombie> _pool;
    private Dictionary<ZombieState, IZombieState> _states;
    private IZombieState _currentState;
    private ZombieHitState _hitState;
    private ZombieState _state = ZombieState.Idle;
    private Vector3 _startPosition;
    private float _currentHealth;
    private bool _spawnInitialized;
    private readonly List<GameObject> _bloodSprays = new();
    private NavMeshAgent _agent;
    private Collider _collider;
    private Animator _animator;

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
    public float PatrolMoveSpeed => _patrolMoveSpeed;
    public float MoveSpeed => _moveSpeed;
    public float ArrivalDistance => _arrivalDistance;
    public float NavMeshSampleDistance => _navMeshSampleDistance;
    public GameObject Player => _player;
    public GameObject BloodSprayPrefab => _bloodSprayPrefab;
    public Vector3 StartPosition => _startPosition;
    public IPool<Zombie> Pool => _pool;
    public NavMeshAgent Agent => _agent;
    public Collider BodyCollider => _collider;
    public bool CanNavigate => _agent != null && _agent.isActiveAndEnabled && _agent.isOnNavMesh;

    public event Action OnDamaged;
    public event Action<Zombie> OnDeath;

    private void Awake()
    {
        // 길을 찾아 걷게해주는 컴포넌트 NavMeshAgent를 찾아서 참조
        _agent = GetComponent<NavMeshAgent>();
        _agent.speed = _moveSpeed;
        _collider = GetComponent<CapsuleCollider>();
        _animator = GetComponentInChildren<Animator>();

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
            StopMoving();
            return;
        }

        _currentState?.UpdateState();
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

        _currentState?.ExitState();
        StopMoving();

        _state = nextState;
        _currentState = _states[nextState];

        _currentState.EnterState();
    }

    public bool Spawn(IPool<Zombie> pool, Vector3 position, Quaternion rotation)
    {
        _pool = pool;
        // 풀에서 꺼낸 Agent를 켜 둔 채 Transform만 순간이동시키지 않음
        StopMoving();
        _agent.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        return ResetForSpawn();
    }

    public bool ResetForSpawn()
    {
        // 같은 Idle로 재사용돼도 진입 처리와 타이머 초기화를 다시 실행
        _currentState?.ExitState();
        _currentState = null;
        ClearBloodSprays();

        _spawnInitialized = true;
        _agent.enabled = false;
        _collider.enabled = false;
        NavMeshQueryFilter filter = new()
        {
            agentTypeID = _agent.agentTypeID,
            areaMask = _agent.areaMask
        };
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit,
            _navMeshSampleDistance, filter))
        {
            Debug.LogWarning("좀비 스폰 위치 주변에 맞는 NavMesh가 없습니다. Surface 베이크와 스폰 위치를 확인하세요.", this);
            return false;
        }

        // 캡슐 중심이 원점인 현재 프리팹은 Base Offset만큼 바닥 위에 배치
        transform.position = hit.position + Vector3.up * _agent.baseOffset;
        _agent.enabled = true;
        if (!_agent.isOnNavMesh)
        {
            _agent.enabled = false;
            Debug.LogWarning("좀비 Agent를 NavMesh에 연결하지 못했습니다.", this);
            return false;
        }

        _agent.speed = _moveSpeed;
        _collider.enabled = true;
        _currentHealth = _maxHealth;
        _startPosition = transform.position;
        _player = GameObject.FindGameObjectWithTag("Player");

        ChangeState(ZombieState.Idle);
        // 재사용된 좀비는 애니메이터 상태와 남은 트리거를 모두 지우고 기본 상태(Idle)에서 시작
        _animator.Rebind();
        _animator.Update(0f);
        OnDamaged?.Invoke();
        return true;
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

    public bool MoveTo(Vector3 target)
    {
        if (!CanNavigate)
        {
            return false;
        }
        // 길 찾기와 이동 중 회전은 Agent가 담당
        _agent.isStopped = false;
        return _agent.SetDestination(target);
    }

    public void StopMoving()
    {
        if (!CanNavigate)
        {
            return;
        }
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        _agent.isStopped = true;
    }

    public bool HasReachedDestination()
    {
        if (!CanNavigate || _agent.pathPending ||
            _agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            return false;
        }

        return !float.IsInfinity(_agent.remainingDistance) &&
            _agent.remainingDistance <= Mathf.Max(_arrivalDistance, _agent.stoppingDistance);
    }

    public void FaceTo(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.forward = direction.normalized;
    }

    public float GetPlayerDistance()
    {
        return _player == null
            ? float.PositiveInfinity
            : Vector3.Distance(
                transform.position,
                _player.transform.position);
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

    public void SetAnimTrigger(string triggerName)
    {
        _animator.SetTrigger(triggerName);
    }

    public void ResetAnimTrigger(string triggerName)
    {
        _animator.ResetTrigger(triggerName);
    }

    public void AttackAction()
    {
        // 피격·사망으로 상태가 바뀐 뒤 늦게 도착한 이벤트는 무시
        if (_state != ZombieState.Attack)
        {
            return;
        }

        ((ZombieAttackState)_states[ZombieState.Attack]).AttackAction();
    }

}
