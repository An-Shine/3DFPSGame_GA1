using UnityEngine;
using System.Collections;

// 마우스 왼쪽버튼을 누르면 바라보는 방향으로 총알을 발사하고 싶음
public class PlayerGunFire : MonoBehaviour
{
    [SerializeField] private ParticleSystem _hitEffectPrefab;

    [SerializeField] private float _range = 100f;
    [SerializeField] private float _damage = 10f;
    [SerializeField] private float _fireInterval = 0.1f;
    [SerializeField] private int _bulletAmmo = 30;
    [SerializeField] private float _reloadTime = 1.5f;

    [SerializeField] private GameObject[] _muzzleFlashes;
    [SerializeField] private GameObject[] _rightMuzzleFlashes;
    [SerializeField] private float _flashTime = 0.05f;

    [SerializeField] private LineRenderer[] _laserLines;
    [SerializeField] private Transform[] _laserStarts;
    [SerializeField] private float _laserDuration = 0.05f;

    private int _remainingBulletAmmo;
    private float _fireTimer = 0f;
    private Coroutine _reloadCoroutine;
    private Animator _animator;
    private float _laserTimer;
    private Vector3 _laserEndPoint;

    public event System.Action<int, int> OnBulletCountChanged;
    public int RemainingBulletAmmo => _remainingBulletAmmo;
    public int BulletAmmo => _bulletAmmo;

    private void Awake()
    {
        _remainingBulletAmmo = _bulletAmmo;
        _animator = GetComponentInChildren<Animator>();
        foreach (LineRenderer line in _laserLines)
        {
            line.positionCount = 2;
            line.useWorldSpace = true;
            line.enabled = false;
        }
    }
    private void Update()
    {
        _laserTimer = Mathf.Max(0f, _laserTimer - Time.deltaTime);
        if (GameManager.Instance.State != GameState.Playing)
        {
            _laserTimer = 0f;
            return;
        }
        _fireTimer -= Time.deltaTime;
        bool isMouseVisible = Cursor.visible;
        bool isFiring = !isMouseVisible && Input.GetButton("Fire1") && _remainingBulletAmmo > 0;
        _animator.SetBool("Fire", isFiring);
        // 1. 마우스 왼쪽버튼 입력받기
        if (!isMouseVisible && Input.GetButton("Fire1") && _fireTimer <= 0f && _remainingBulletAmmo > 0)
        {
            CancelReload();
            _fireTimer = _fireInterval;
            Shoot();
        }
        if (Input.GetKeyDown(KeyCode.R) && _reloadCoroutine == null && _remainingBulletAmmo < _bulletAmmo)
        {
            _reloadCoroutine = StartCoroutine(Reloading_Coroutine());
        }
    }

    private void Shoot()
    {
        StartCoroutine(MuzzleFlash_Coroutine());
        // 2. 레이(광선) 생성, 발사될 위치와 진행방향 설정하기
        Transform cameraTransform = Camera.main.transform;
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

        // 3. 레이가 부딪힌 대상의 정보를 저장하기
        RaycastHit hitInfo = new RaycastHit();

        _remainingBulletAmmo--;
        OnBulletCountChanged?.Invoke(_remainingBulletAmmo, _bulletAmmo);

        // 4. 레이를 발사하고, 만일 부딪힌 물체가 있다면 이펙트 표시하기
        bool isHit = Physics.Raycast(ray, out hitInfo, _range);
        // 명중하면 충돌 지점까지, 빗나가면 최대 사거리까지 표시한다.
        // 명중 판정은 한 번만 하고, 두 총구의 레이저가 같은 지점에 모이게 한다.
        _laserEndPoint = isHit ? hitInfo.point : ray.GetPoint(_range);

        // 발사할 때마다 표시 시간을 다시 채운다.
        _laserTimer = _laserDuration;

        if (isHit)
        {
            ParticleSystem effect = Instantiate(_hitEffectPrefab);
            effect.transform.position = hitInfo.point; // 맞은 자리
            effect.transform.forward = hitInfo.normal; // 수직 위치로 회전
            effect.Play();
            DamageInfo damageInfo = new();
            damageInfo.Value = _damage;
            damageInfo.HitPoint = hitInfo.point;
            damageInfo.HitNormal = hitInfo.normal;
            hitInfo.collider.GetComponent<IDamageable>()?.TakeDamage(damageInfo);
        }
    }

    private void LateUpdate()
    {
        // 표시 중에도 총구 위치를 따라가므로 이동하거나 회전해도 레이저가 분리되지 않는다.
        for (int i = 0; i < _laserLines.Length; i++)
        {
            _laserLines[i].enabled = _laserTimer > 0f;
            if (_laserLines[i].enabled)
            {
                _laserLines[i].SetPosition(0, _laserStarts[i].position);
                _laserLines[i].SetPosition(1, _laserEndPoint);
            }
        }
    }

    private IEnumerator Reloading_Coroutine()
    {
        yield return new WaitForSeconds(_reloadTime);
        _remainingBulletAmmo = _bulletAmmo;
        _reloadCoroutine = null;
        OnBulletCountChanged?.Invoke(_remainingBulletAmmo, _bulletAmmo);
    }

    private void CancelReload()
    {
        if (_reloadCoroutine == null)
        {
            return;
        }
        StopCoroutine(_reloadCoroutine);
        _reloadCoroutine = null;
    }

    private IEnumerator MuzzleFlash_Coroutine()
    {
        int randomIndex = UnityEngine.Random.Range(0, _muzzleFlashes.Length);
        GameObject flash = _muzzleFlashes[randomIndex];
        GameObject rightFlash = _rightMuzzleFlashes[randomIndex];
        // 기존 총구 섬광을 양쪽에서 동시에 보여 준다. 탄약과 대미지는 추가하지 않는다.
        flash.SetActive(true);
        rightFlash.SetActive(true);
        yield return new WaitForSeconds(_flashTime);
        flash.SetActive(false);
        rightFlash.SetActive(false);
    }
}
