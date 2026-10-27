using TMPro;
using UnityEngine;

public class ZombieIdleState : IZombieState
{
    private readonly Zombie _zombie;
    private readonly ZombiePatrolState _patrolState;
    private float _timer;

    public ZombieIdleState(Zombie zombie, ZombiePatrolState patrolState)
    {
        _zombie = zombie;
        _patrolState = patrolState;
    }

    public void EnterState()
    {
        _timer = 0f;
        _zombie.StopMoving();
        _zombie.ResetAnimTrigger("IdleToMove");
        _zombie.ResetAnimTrigger("IdleToPatrol");
        _zombie.SetAnimTrigger("MoveToIdle");
    }

    public void UpdateState()
    {
        if (_zombie.GetPlayerDistance() <= _zombie.DetectionDistance)
        {
            _zombie.ChangeState(ZombieState.Trace);
            return;
        }

        _timer += Time.deltaTime;
        if (_timer < _zombie.IdleDuration)
        {
            return;
        }

        // 경로를 못 찾으면 Idle을 유지하고 다음 대기 시간 뒤 재시도
        _timer = 0f;
        if (_patrolState.TryPrepareRoute())
        {
            _zombie.ChangeState(ZombieState.Patrol);
        }
    }

    public void ExitState()
    {
        _zombie.StopMoving();
    }
}
