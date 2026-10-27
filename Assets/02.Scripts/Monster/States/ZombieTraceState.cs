using UnityEngine;

public class ZombieTraceState : IZombieState
{
    private readonly Zombie _zombie;

    public ZombieTraceState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void EnterState()
    {
    }

    public void UpdateState()
    {
        if (_zombie.Player == null)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        float distanceFromStart = Vector3.Distance(
            _zombie.transform.position, _zombie.StartPosition);
        if (distanceFromStart > _zombie.ComebackDistance)
        {
            _zombie.ChangeState(ZombieState.Comeback);
            return;
        }

        if (_zombie.GetPlayerDistance() <= _zombie.AttackRange)
        {
            _zombie.ChangeState(ZombieState.Attack);
            return;
        }

        _zombie.MoveTo(_zombie.Player.transform.position);
    }

    public void ExitState()
    {
        _zombie.StopMoving();
    }
}
