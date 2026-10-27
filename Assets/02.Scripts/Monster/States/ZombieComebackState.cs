public class ZombieComebackState : IZombieState
{
    private readonly Zombie _zombie;
    private bool _hasDestination;

    public ZombieComebackState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void EnterState()
    {
        // 복귀 경로를 먼저 요청한 뒤 도착 여부를 확인
        _hasDestination = _zombie.MoveTo(_zombie.StartPosition);
    }

    public void UpdateState()
    {
        if (!_hasDestination || !_zombie.CanNavigate)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        if (_zombie.Agent.pathPending)
        {
            return;
        }

        // 게임 일시 정지 등으로 경로를 지웠다면 복귀 목적지를 다시 요청
        if (_zombie.Agent.isStopped)
        {
            _hasDestination = _zombie.MoveTo(_zombie.StartPosition);
            return;
        }

        if (_zombie.Agent.pathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        if (_zombie.HasReachedDestination())
        {
            _zombie.ChangeState(ZombieState.Idle);
        }
    }

    public void ExitState()
    {
        _zombie.StopMoving();
    }
}
