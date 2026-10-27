using UnityEngine;

public class ZombieHitState : IZombieState
{
    private readonly Zombie _zombie;
    private DamageInfo _damageInfo;
    private float _timer;

    public ZombieHitState(Zombie zombie)
    {
        _zombie = zombie;
    }

    public void SetDamageInfo(DamageInfo damageInfo)
    {
        _damageInfo = damageInfo;
    }

    public void EnterState()
    {
        _timer = 0f;
        _zombie.StopMoving();
        CreateBloodSpray();
    }

    private void CreateBloodSpray()
    {
        GameObject prefab = _zombie.BloodSprayPrefab;
        if (prefab == null)
        {
            return;
        }

        GameObject bloodSpray = Object.Instantiate(prefab, _zombie.transform);
        bloodSpray.transform.position = _damageInfo.HitPoint;
        if (_damageInfo.HitNormal.sqrMagnitude > 0.0001f)
        {
            bloodSpray.transform.forward = _damageInfo.HitNormal;
        }

        _zombie.RegisterBloodSpray(bloodSpray);
    }

    public void UpdateState()
    {
        _timer += Time.deltaTime;
        if (_timer < 0.3f)
        {
            return;
        }

        // 기존 동작 유지: 피격 경직이 끝나면 플레이어를 추적한다.
        _zombie.ChangeState(ZombieState.Trace);
    }

    public void ExitState()
    {
        _zombie.StopMoving();
    }
}
