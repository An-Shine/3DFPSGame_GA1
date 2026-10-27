using UnityEngine;

public class ZombieDeathState : IZombieState
{
    private readonly Zombie _zombie;
    private float _timer;
    private bool _completed;

    public ZombieDeathState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void EnterState()
    {
        _timer = 0f;
        _completed = false;
        _zombie.StopMoving();
        _zombie.Controller.enabled = false;

        // 사망 알림은 진입할 때 한 번만 보냄
        _zombie.NotifyDeath();
    }

    public void UpdateState()
    {
        if (_completed)
        {
            return;
        }

        _timer += Time.deltaTime;
        if (_timer < 0.5f)
        {
            return;
        }

        // 반환으로 비활성화되기 전에 중복 실행을 차단
        _completed = true;
        CompleteDeath();
    }

    private void CompleteDeath()
    {
        _zombie.ClearBloodSprays();
        if (_zombie.Pool != null)
        {
            _zombie.Pool.Take(_zombie);
        }
        else
        {
            Object.Destroy(_zombie.gameObject);
        }
    }

    public void ExitState()
    {
    }
}
