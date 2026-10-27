public class ZombieComebackState : IZombieState
{
    private readonly Zombie _zombie;

    public ZombieComebackState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void EnterState()
    {
    }

    public void UpdateState()
    {
        float distance = _zombie.GetHorizontalDistance(
            _zombie.transform.position, _zombie.StartPosition);
        if (distance <= _zombie.ArrivalDistance)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        _zombie.MoveTo(_zombie.StartPosition);
    }

    public void ExitState()
    {
        _zombie.StopMoving();
    }
}
