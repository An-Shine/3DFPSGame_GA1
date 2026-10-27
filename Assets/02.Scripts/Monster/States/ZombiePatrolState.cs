using UnityEngine;
using UnityEngine.AI;

public class ZombiePatrolState : IZombieState
{
    private readonly Zombie _zombie;
    private readonly NavMeshPath _path = new();
    private Vector3 _pointA;
    private Vector3 _pointB;
    private Vector3 _target;
    private Vector3 _progressPosition;
    private float _waitTimer;
    private float _progressTimer;
    private bool _waiting;
    private bool _hasDestination;

    public ZombiePatrolState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public bool TryPrepareRoute()
    {
        if (!_zombie.CanNavigate)
        {
            return false;
        }

        NavMeshQueryFilter filter = new()
        {
            agentTypeID = _zombie.Agent.agentTypeID,
            areaMask = _zombie.Agent.areaMask
        };

        if (!NavMesh.SamplePosition(_zombie.transform.position, out NavMeshHit start,
            _zombie.NavMeshSampleDistance, filter))
        {
            return false;
        }

        _pointA = start.position;
        for (int i = 0; i < 10; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 candidate = _pointA + direction * _zombie.PatrolDistance;

            // NavMesh 위의 위치라도 현재 좀비에서 도달 가능한지 별도로 검사
            if (!NavMesh.SamplePosition(candidate, out NavMeshHit hit,
                _zombie.NavMeshSampleDistance, filter))
            {
                continue;
            }

            float minDistance = Mathf.Max(_zombie.ArrivalDistance,
                _zombie.Agent.stoppingDistance) * 2f;
            if (_zombie.GetHorizontalDistance(_pointA, hit.position) <= minDistance)
            {
                continue;
            }

            if (!_zombie.Agent.CalculatePath(hit.position, _path) ||
                _path.status != NavMeshPathStatus.PathComplete)
            {
                continue;
            }

            _pointB = hit.position;
            _target = _pointB;
            return true;
        }

        return false;
    }

    public void EnterState()
    {
        _waitTimer = 0f;
        _waiting = false;
        ResetProgress();
        _hasDestination = _zombie.MoveTo(_target);
    }

    public void UpdateState()
    {
        if (_zombie.GetPlayerDistance() <= _zombie.DetectionDistance)
        {
            _zombie.ChangeState(ZombieState.Trace);
            return;
        }

        if (!_zombie.CanNavigate)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        if (_waiting)
        {
            _waitTimer += Time.deltaTime;
            if (_waitTimer >= _zombie.PatrolWaitDuration)
            {
                _target = _target == _pointB ? _pointA : _pointB;
                _waitTimer = 0f;
                _waiting = false;
                ResetProgress();
                _hasDestination = _zombie.MoveTo(_target);
            }

            return;
        }

        if (!_hasDestination)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        // 경로 계산 중인 시간을 도착이나 이동 막힘으로 판단하지 않음
        if (_zombie.Agent.pathPending)
        {
            ResetProgress();
            return;
        }

        if (_zombie.Agent.isStopped)
        {
            ResetProgress();
            _hasDestination = _zombie.MoveTo(_target);
            return;
        }

        if (_zombie.Agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        if (_zombie.HasReachedDestination())
        {
            _zombie.StopMoving();
            _waiting = true;
            _waitTimer = 0f;
            ResetProgress();
            return;
        }

        CheckProgress();
    }

    private void CheckProgress()
    {
        _progressTimer += Time.deltaTime;
        if (_progressTimer < 1f)
        {
            return;
        }

        float movedDistance = _zombie.GetHorizontalDistance(
            _zombie.transform.position, _progressPosition);
        if (movedDistance < 0.1f)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        ResetProgress();
    }

    private void ResetProgress()
    {
        _progressTimer = 0f;
        _progressPosition = _zombie.transform.position;
    }

    public void ExitState()
    {
        _zombie.StopMoving();
    }
}
