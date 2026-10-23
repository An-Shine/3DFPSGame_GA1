using System.Collections;
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

//FSM 유한상태머신 : 유한개의 상태가 있고, 상태마다 행동을 다르게 함으로써 AI를 구현하는 유명한 방법
// 상태 : 객체가 가질 수 있는 어떤 조건이나 상황
// 전이 : 조건에 따라 현재 상태에서 다른 상태로 넘어가는 것
// 그리고 위 방식에 따라 규칙을 세워둔 것을 "머신" 

// 좀비가 가질 수 있는 상태

[RequireComponent(typeof(CharacterController))]
public class Zombie : MonoBehaviour
{
    [Header("스탯")]
    [SerializeField] private float _detectionDistance = 8f;
    [SerializeField] private float _comebackDistance = 15f;
    [SerializeField] private float _attackRange = 1.5f;
    [SerializeField] private float _attackInterval = 1.2f;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _health = 30f;
    [SerializeField] private float _damage = 10f;

    [Header("순찰")]
    [SerializeField, Min(0f)] private float _idleDuration = 2f;
    [SerializeField, Min(0.5f)] private float _patrolDistance = 4f;
    [SerializeField, Min(0f)] private float _patrolWaitDuration = 1f;
    [SerializeField, Min(0.05f)] private float _arrivalDistance = 0.2f;

    [Header("순찰 지점 검사")]
    [SerializeField] private LayerMask _groundMask = 1 << 8;
    [SerializeField] private LayerMask _obstacleMask = 1 << 9;

    private CharacterController _controller;
    private GameObject _player;
    private ZombieState _state = ZombieState.Idle;
    private Vector3 _startPosition;
    private Vector3 _patrolPointA;
    private Vector3 _patrolPointB;
    private Vector3 _patrolTarget;
    private Vector3 _moveDirection;
    private Vector3 _progressPosition;
    private float _currentHealth;
    private float _idleTimer;
    private float _patrolWaitTimer;
    private float _attackTimer;
    private float _progressTimer;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        ResetForSpawn();
    }

    // 랜덤 스폰이나 풀 재사용 시에도 위치를 옮긴 다음 호출한다.
    public void ResetForSpawn()
    {
        StopAllCoroutines();
        _controller.enabled = true;
        _currentHealth = _health;
        _startPosition = transform.position;
        _player = GameObject.FindGameObjectWithTag("Player");
        ChangeState(ZombieState.Idle);
        ResetTimers();
    }

    private void Update()
    {
        if (_state == ZombieState.Death)
        {
            return;
        }

        _moveDirection = Vector3.zero;
        switch (_state)
        {
            case ZombieState.Idle:
                Idle();
                break;
            case ZombieState.Patrol:
                Patrol();
                break;
            case ZombieState.Trace:
                Trace();
                break;
            case ZombieState.Comeback:
                Comeback();
                break;
            case ZombieState.Attack:
                Attack();
                break;
        }

        // 대기·공격·피격 중에도 중력을 적용하고 실제 이동은 한 번만 처리한다.
        _controller.SimpleMove(_moveDirection * _moveSpeed);
    }

    private void Idle()
    {
        // 순찰 시작보다 플레이어 발견을 우선한다.
        if (GetPlayerDistance() <= _detectionDistance)
        {
            ChangeState(ZombieState.Trace);
            return;
        }

        _idleTimer += Time.deltaTime;
        if (_idleTimer < _idleDuration)
        {
            return;
        }

        // 지점을 못 찾으면 Idle을 유지하고 2초 뒤 다시 시도한다.
        _idleTimer = 0f;
        if (TrySetPatrolPoints())
        {
            ChangeState(ZombieState.Patrol);
        }
    }

    private void Patrol()
    {
        if (GetPlayerDistance() <= _detectionDistance)
        {
            ChangeState(ZombieState.Trace);
            return;
        }

        if (GetHorizontalDistance(transform.position, _patrolTarget) > _arrivalDistance)
        {
            // 새 장애물이 생겼으면 Idle로 돌아가 다른 구간을 찾는다.
            if (!IsPatrolPathClear(transform.position, _patrolTarget))
            {
                ChangeState(ZombieState.Idle);
                return;
            }

            MoveTo(_patrolTarget);
            CheckPatrolProgress();
            return;
        }

        // 도착 후 1초는 Patrol 상태로 기다린다. 이때 막힘 검사는 중단한다.
        _progressTimer = 0f;
        _progressPosition = transform.position;
        _patrolWaitTimer += Time.deltaTime;
        if (_patrolWaitTimer < _patrolWaitDuration)
        {
            return;
        }

        _patrolWaitTimer = 0f;
        _patrolTarget = _patrolTarget == _patrolPointB ? _patrolPointA : _patrolPointB;
    }

    private void Trace()
    {
        if (_player == null)
        {
            ChangeState(ZombieState.Idle);
            return;
        }

        if (Vector3.Distance(transform.position, _startPosition) > _comebackDistance)
        {
            ChangeState(ZombieState.Comeback);
            return;
        }

        if (GetPlayerDistance() <= _attackRange)
        {
            ChangeState(ZombieState.Attack);
            return;
        }

        MoveTo(_player.transform.position);
    }

    private void Attack()
    {
        if (GetPlayerDistance() > _attackRange)
        {
            ChangeState(ZombieState.Trace);
            return;
        }

        _attackTimer += Time.deltaTime;
        if (_attackTimer >= _attackInterval)
        {
            // TODO: 플레이어에게 _damage만큼 실제 피해 전달
            Debug.Log("플레이어를 공격했습니다");
            _attackTimer = 0f;
        }
    }

    private void Comeback()
    {
        if (GetHorizontalDistance(transform.position, _startPosition) <= _arrivalDistance)
        {
            ChangeState(ZombieState.Idle);
            return;
        }

        MoveTo(_startPosition);
    }

    private void MoveTo(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        transform.forward = direction.normalized;
        // 목적지 직전에는 이동량을 줄여 지나치지 않게 한다.
        float step = _moveSpeed * Time.deltaTime;
        float ratio = step > 0f ? Mathf.Clamp01(direction.magnitude / step) : 0f;
        _moveDirection = direction.normalized * ratio;
    }

    private float GetPlayerDistance()
    {
        return _player == null
            ? float.PositiveInfinity
            : Vector3.Distance(transform.position, _player.transform.position);
    }

    private float GetHorizontalDistance(Vector3 from, Vector3 to)
    {
        Vector3 direction = to - from;
        direction.y = 0f;
        return direction.magnitude;
    }

    private bool TrySetPatrolPoints()
    {
        _patrolPointA = transform.position;
        // 후보 선택은 최대 10번만 시도한다.
        for (int i = 0; i < 10; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 candidate = _patrolPointA + direction * _patrolDistance;
            if (!IsPatrolPathClear(_patrolPointA, candidate))
            {
                continue;
            }

            _patrolPointB = candidate;
            _patrolTarget = _patrolPointB;
            return true;
        }

        return false;
    }

    private bool IsPatrolPathClear(Vector3 from, Vector3 to)
    {
        // Scale (1, 1, 1)인 좀비와 높이 차이가 작은 바닥을 기준으로 검사한다.
        Vector3 direction = to - from;
        direction.y = 0f;
        float distance = direction.magnitude;
        Vector3 center = from + _controller.center;
        float radius = _controller.radius;
        float offset = Mathf.Max(0f, _controller.height * 0.5f - radius);
        Vector3 bottom = center - Vector3.up * offset;
        Vector3 top = center + Vector3.up * offset;

        // 시작점의 겹침과 몸이 지나갈 공간을 각각 확인한다.
        if (Physics.CheckCapsule(bottom, top, radius, _obstacleMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (distance > 0f && Physics.CapsuleCast(bottom, top, radius,
            direction.normalized, distance, _obstacleMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        // 바닥을 0.5m 이하 간격으로 확인한다. 정밀한 낭떠러지 탐색은 아니다.
        int steps = Mathf.Max(1, Mathf.CeilToInt(distance / 0.5f));
        float feetOffset = _controller.center.y - _controller.height * 0.5f;
        for (int i = 0; i <= steps; i++)
        {
            Vector3 position = Vector3.Lerp(from, to, (float)i / steps);
            Vector3 origin = position + Vector3.up * (feetOffset + 0.3f);
            if (!Physics.Raycast(origin, Vector3.down, 0.6f,
                _groundMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }
        }

        return true;
    }

    private void CheckPatrolProgress()
    {
        // 다른 캐릭터 등에 막혀 실제로 전진하지 못한 경우도 대기로 돌린다.
        _progressTimer += Time.deltaTime;
        if (_progressTimer < 1f)
        {
            return;
        }

        if (GetHorizontalDistance(transform.position, _progressPosition) < 0.1f)
        {
            ChangeState(ZombieState.Idle);
            return;
        }

        _progressTimer = 0f;
        _progressPosition = transform.position;
    }

    private void ChangeState(ZombieState nextState)
    {
        if (_state == nextState)
        {
            return;
        }

        Debug.Log($"{_state} -> {nextState}");
        _state = nextState;
        ResetTimers();
    }

    private void ResetTimers()
    {
        _idleTimer = 0f;
        _patrolWaitTimer = 0f;
        _attackTimer = 0f;
        _progressTimer = 0f;
        _progressPosition = transform.position;
        _moveDirection = Vector3.zero;
    }

    public void TakeDamage(float damage)
    {
        if (_state == ZombieState.Death)
        {
            return;
        }

        _currentHealth -= Mathf.Max(0f, damage);
        // 연속 피격 시 이전의 Trace 복귀 예약을 취소한다.
        StopAllCoroutines();
        if (_currentHealth > 0f)
        {
            ChangeState(ZombieState.Hit);
            StartCoroutine(Hit_Coroutine());
        }
        else
        {
            ChangeState(ZombieState.Death);
            StartCoroutine(Death_Coroutine());
        }
    }

    private IEnumerator Hit_Coroutine()
    {
        yield return new WaitForSeconds(0.3f);
        ChangeState(ZombieState.Trace);
    }

    private IEnumerator Death_Coroutine()
    {
        _controller.enabled = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
