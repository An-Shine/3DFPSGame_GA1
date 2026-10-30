using UnityEngine;

public class PlayerMeleeAttack : MonoBehaviour
{
    [SerializeField] private GameObject _katana;

    private Player _player;
    private Animator _animator;
    private int _meleeLayerIndex;

    [SerializeField] private Collider _katanaCollider;
    [SerializeField] private MeleeHitMode _hitMode = MeleeHitMode.Collider; // 맞힐 대상을 고르는 방법
    [SerializeField] private float _fanDamage = 25f; // 부채꼴 방법의 대미지
    [SerializeField] private float _range = 2f; // 부채꼴의 반지름 (m)
    [SerializeField][Range(0f, 360f)] private float _angle = 90f; // 부채꼴 각도 (도)
    [SerializeField] private LayerMask _targetMask; // 때릴 대상 (Drum, Zombie)
    [SerializeField] private LayerMask _obstacleMask; // 가로막는 것 (Default)

    private void Awake()
    {
        _player = GetComponentInParent<Player>();
        _animator = GetComponentInChildren<Animator>();
        // 오브젝트 레이어(LayerMask)가 아니라 Animator 컨트롤러의 레이어 인덱스를 구해야 한다.
        _meleeLayerIndex = _animator.GetLayerIndex("Melee");
        _katanaCollider = _katana.GetComponentInChildren<Collider>(true);
    }

    private void OnEnable()
    {
        _player.OnModeChanged += RefreshKatana;
    }

    private void OnDisable()
    {
        _player.OnModeChanged -= RefreshKatana;
    }

    private void Start()
    {
        RefreshKatana(_player.WeaponMode);
    }

    public void ActivateCollider()
    {
        if (_hitMode != MeleeHitMode.Collider) return;
        Debug.Log("카타나 콜라이더 활성화");
        _katanaCollider.enabled = true;
    }

    public void DeactivateCollider()
    {
        Debug.Log("카타나 콜라이더 비활성화");
        _katanaCollider.enabled = false;
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        // Fire2를 누르면 Slash 트리거를 켠다. Empty → Attack1(slash1) → Empty 한 번만 재생된다.
        if (Input.GetButtonDown("Fire2") && _player.WeaponMode == WeaponMode.Melee && IsResting())
        {
            DeactivateCollider();
            _animator.SetTrigger("Slash");
        }
    }

    private bool IsResting()
    {
        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(_meleeLayerIndex);
        return state.IsName("Empty") && !_animator.IsInTransition(_meleeLayerIndex);
    }

    // 무기 모드가 바뀌었다는 알림이 오면 불리는 함수(콜백). 카타나는 근접 모드에서만 보인다.
    private void RefreshKatana(WeaponMode mode)
    {
        _katana.SetActive(mode == WeaponMode.Melee);
    }

    // 앞쪽 부채꼴 안의 대상을 한 번에 때린다. slash8의 FanAttack 이벤트가 불러 준다.
    public void FanAttack()
    {
        // 칼날 콜라이더 방법일 때는 하지 않는다.
        if (_hitMode != MeleeHitMode.Fan) return;

        // Justyna의 발밑이 아니라 Player의 가운데에서, Player의 앞쪽으로 잰다.
        Transform player = _player.transform;
        Vector3 origin = player.position;

        // 1. 사거리 안의 대상을 모두 찾는다.
        Collider[] colliders = Physics.OverlapSphere(origin, _range, _targetMask);
        foreach (Collider col in colliders)
        {
            // 2. 대상에서 플레이어에 가장 가까운 점을 맞을 자리로 삼는다.
            Vector3 hitPoint = col.ClosestPoint(origin);

            // 3. 높이를 뺀 방향이 앞쪽에서 Angle의 절반 안에 있는지 본다.
            Vector3 direction = hitPoint - origin;
            direction.y = 0f;
            if (Vector3.Angle(player.forward, direction) > _angle / 2f) continue;

            // 4. 가는 길에 벽이 있으면 때리지 않는다.
            if (Physics.Linecast(origin, hitPoint, _obstacleMask)) continue;

            // 5. 대미지를 받을 수 있으면 대미지를 준다.
            if (col.TryGetComponent(out IDamageable damageable))
            {
                DamageInfo damageInfo = new();
                damageInfo.Value = _fanDamage;
                damageInfo.HitPoint = hitPoint;
                damageInfo.HitNormal = (origin - hitPoint).normalized;
                damageable.TakeDamage(damageInfo);
            }
        }
    }
}