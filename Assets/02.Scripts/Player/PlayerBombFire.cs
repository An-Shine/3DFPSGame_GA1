using UnityEngine;
using TMPro;
using Redcode.Pools;
using UnityEngine.InputSystem.Layouts;

public class PlayerBombFire : MonoBehaviour
{
    [SerializeField] private Transform _firePosition;
    [SerializeField] private PoolManager _poolManager;
    [SerializeField] private float _throwPower = 15f;
    [SerializeField] private float _maxThrowPower = 30f;
    [SerializeField] private int _bombCount = 10;
    [SerializeField] private TMP_Text _bombCountUI;

    [Min(0.01f)][SerializeField] private float _maxChargeTime = 2f;

    [Header("투척 애니메이션 (Grenade Throw 클립 프레임 기준)")]
    [SerializeField] private int _clipLastFrame = 83;       // 클립의 마지막 프레임 (30fps, 0~83)
    [SerializeField] private int _holdFrame = 30;           // 차지 중 멈춰서 대기하는 프레임
    [SerializeField] private int _releaseFrame = 62;        // 폭탄이 손을 떠나는 프레임
    [SerializeField] private float _releaseTimeout = 3f;    // 애니메이션이 진행되지 않을 때 강제로 던지기까지의 시간

    private const string ThrowLayerName = "Throw";
    private const string ThrowStateName = "Throw";

    private float _chargeTime;
    private bool _isCharging = false;
    private bool _isHolding = false;         // 애니메이션을 대기 프레임에서 멈춘 상태
    private bool _isWaitingRelease = false;  // 우클릭을 뗐고 발사 프레임을 기다리는 중
    private float _releasePower;
    private float _releaseTimer;
    private IPool<Bomb> _bombPool;
    private Animator _animator;
    private int _throwLayerIndex;

    private float HoldNormalizedTime => (float)_holdFrame / _clipLastFrame;
    private float ReleaseNormalizedTime => (float)_releaseFrame / _clipLastFrame;

    private void Awake()
    {
        ShowBombCount();
        _animator = GetComponentInChildren<Animator>();
        _throwLayerIndex = _animator.GetLayerIndex(ThrowLayerName);
    }

    private void Start()
    {
        _bombPool = _poolManager.GetPool<Bomb>("Bomb");
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        // 이전 투척 동작(발사 대기, 후속 동작)이 끝나기 전에는 새로 시작하지 않는다
        if (Input.GetButtonDown("Fire2") && _bombCount > 0 && !_isWaitingRelease && !IsPlayingThrowState(out _))
        {
            StartCharge();
        }

        if (_isCharging)
        {
            if (Input.GetButton("Fire2"))
            {
                _chargeTime = Mathf.Min(_chargeTime + Time.deltaTime, _maxChargeTime);
            }

            HoldChargePose();

            if (Input.GetButtonUp("Fire2"))
            {
                ReleaseCharge();
            }
        }
        else if (_isWaitingRelease)
        {
            WaitReleaseFrame();
        }
    }

    private void StartCharge()
    {
        _chargeTime = 0f;
        _isCharging = true;
        _isHolding = false;
        _animator.SetFloat("ThrowSpeed", 1f);
        _animator.SetBool("Throw", true);
    }

    // 던지기 클립이 대기 프레임에 도달하면 우클릭을 뗄 때까지 멈춘다
    private void HoldChargePose()
    {
        if (_isHolding) return;

        if (IsPlayingThrowState(out float normalizedTime) && normalizedTime >= HoldNormalizedTime)
        {
            _animator.SetFloat("ThrowSpeed", 0f);
            _isHolding = true;
        }
    }

    // 우클릭을 떼면 애니메이션을 이어서 재생하고, 발사는 발사 프레임에 도달했을 때 한다
    private void ReleaseCharge()
    {
        _isCharging = false;
        _isHolding = false;
        _releasePower = _throwPower + (_maxThrowPower - _throwPower) * (_chargeTime / _maxChargeTime);
        _chargeTime = 0f;
        _releaseTimer = 0f;
        _isWaitingRelease = true;

        _animator.SetBool("Throw", false);
        _animator.SetFloat("ThrowSpeed", 1f);
    }

    private void WaitReleaseFrame()
    {
        _releaseTimer += Time.deltaTime;

        bool isReleaseFrame = IsPlayingThrowState(out float normalizedTime) && normalizedTime >= ReleaseNormalizedTime;
        if (isReleaseFrame || _releaseTimer >= _releaseTimeout)
        {
            _isWaitingRelease = false;
            Shoot(_releasePower);
        }
    }

    // 전환 중이면 들어가는 쪽 상태를 기준으로 Throw 레이어의 재생 상태를 확인한다
    private bool IsPlayingThrowState(out float normalizedTime)
    {
        AnimatorStateInfo state = _animator.IsInTransition(_throwLayerIndex)
            ? _animator.GetNextAnimatorStateInfo(_throwLayerIndex)
            : _animator.GetCurrentAnimatorStateInfo(_throwLayerIndex);

        normalizedTime = state.normalizedTime;
        return state.IsName(ThrowStateName);
    }

    private void Shoot(float power)
    {
        Bomb bomb = _bombPool.Get();
        if (bomb == null)
        {
            return;
        }

        bomb.Launch(_bombPool, _firePosition.position, Camera.main.transform.forward, power);
        _bombCount--;
        ShowBombCount();
    }

    private void ShowBombCount()
    {
        _bombCountUI.text = $"{_bombCount}";
    }
}
