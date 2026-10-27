using UnityEngine;

public class ZombieAttackState : IZombieState
{
    private readonly Zombie _zombie;
    private float _timer;

    public ZombieAttackState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void EnterState()
    {
        _timer = 0f;
        _zombie.StopMoving();
        _zombie.SetAnimTrigger("MoveToAttack");
    }

    public void UpdateState()
    {
        if (_zombie.Player == null)
        {
            _zombie.ChangeState(ZombieState.Idle);
            return;
        }

        if (_zombie.GetPlayerDistance() > _zombie.AttackRange)
        {
            _zombie.ChangeState(ZombieState.Trace);
            return;
        }
        _zombie.FaceTo(_zombie.Player.transform.position);

        _timer += Time.deltaTime;
        if (_timer < _zombie.AttackInterval)
        {
            return;
        }

        _timer = 0f;
        Attack();
    }

    private void Attack()
    {
        // 데미지는 Z_Attack의 애니메이션 이벤트(HitEvent)에서 처리
        _zombie.SetAnimTrigger("StartAttack");
    }

    public void AttackAction()
    {
        GameObject player = _zombie.Player;
        if (player == null)
        {
            return;
        }

        IDamageable target = player.GetComponent<IDamageable>();
        if (target == null)
        {
            return;
        }

        DamageInfo damageInfo = new()
        {
            Value = _zombie.AttackDamage,
            HitPoint = player.transform.position,
            HitNormal = (_zombie.transform.position - player.transform.position).normalized
        };
        target.TakeDamage(damageInfo);
        Debug.Log("플레이어를 공격했습니다");
    }

    public void ExitState()
    {
        _zombie.StopMoving();
        _zombie.SetAnimTrigger("AttackToMove");
    }
}
