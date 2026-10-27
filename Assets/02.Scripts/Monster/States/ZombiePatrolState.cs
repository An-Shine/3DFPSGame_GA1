using UnityEngine;

public class ZombiePatrolState : IZombieState
{
    private readonly Zombie _zombie;
    private Vector3 _pointA;
    private Vector3 _pointB;
    private Vector3 _target;
    private Vector3 _progressPosition;
    private float _waitTimer;
    private float _progressTimer;

    public ZombiePatrolState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public bool TryPrepareRoute()
    {
        _pointA = _zombie.transform.position;
        for (int i = 0; i < 10; i++)
        {
            float angle = Random.Range(0f, 360f);
            Vector3 direction = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            Vector3 candidate = _pointA + direction * _zombie.PatrolDistance;
            if (!IsPathClear(_pointA, candidate))
            {
                continue;
            }

            _pointB = candidate;
            _target = _pointB;
            return true;
        }

        return false;
    }

    public void EnterState()
    {
        // Idle에서 준비한 순찰 지점은 유지하고 시간만 초기화한다.
        _waitTimer = 0f;
        ResetProgress();
    }

    public void UpdateState()
    {
        if (_zombie.GetPlayerDistance() <= _zombie.DetectionDistance)
        {
            _zombie.ChangeState(ZombieState.Trace);
            return;
        }

        float distance = _zombie.GetHorizontalDistance(_zombie.transform.position, _target);
        if (distance > _zombie.ArrivalDistance)
        {
            if (!IsPathClear(_zombie.transform.position, _target))
            {
                _zombie.ChangeState(ZombieState.Idle);
                return;
            }

            _zombie.MoveTo(_target);
            CheckProgress();
            return;
        }

        // 도착 후 1초 대기는 장애물에 막힌 것으로 판단하지 않음
        ResetProgress();
        _waitTimer += Time.deltaTime;
        if (_waitTimer < _zombie.PatrolWaitDuration)
        {
            return;
        }

        _waitTimer = 0f;
        _target = _target == _pointB ? _pointA : _pointB;
    }

    private bool IsPathClear(Vector3 from, Vector3 to)
    {
        // 기존 검사 유지: Scale (1, 1, 1)과 높이 차이가 작은 바닥 기준.
        CharacterController controller = _zombie.Controller;
        Vector3 direction = to - from;
        direction.y = 0f;
        float distance = direction.magnitude;
        Vector3 center = from + controller.center;
        float radius = controller.radius;
        float offset = Mathf.Max(0f, controller.height * 0.5f - radius);
        Vector3 bottom = center - Vector3.up * offset;
        Vector3 top = center + Vector3.up * offset;

        if (Physics.CheckCapsule(bottom, top, radius,
            _zombie.ObstacleMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (distance > 0f && Physics.CapsuleCast(bottom, top, radius,
            direction.normalized, distance, _zombie.ObstacleMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        int steps = Mathf.Max(1, Mathf.CeilToInt(distance / 0.5f));
        float feetOffset = controller.center.y - controller.height * 0.5f;
        for (int i = 0; i <= steps; i++)
        {
            Vector3 position = Vector3.Lerp(from, to, (float)i / steps);
            Vector3 origin = position + Vector3.up * (feetOffset + 0.3f);
            if (!Physics.Raycast(origin, Vector3.down, 0.6f,
                _zombie.GroundMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }
        }

        return true;
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
