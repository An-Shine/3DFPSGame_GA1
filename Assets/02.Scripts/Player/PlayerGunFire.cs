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
    [SerializeField] private float _flashTime = 0.05f;

    private int _remainingBulletAmmo;
    private float _fireTimer = 0f;
    private Coroutine _reloadCoroutine;
    private Animator _animator;

    public event System.Action<int, int> OnBulletCountChanged;
    public int RemainingBulletAmmo => _remainingBulletAmmo;
    public int BulletAmmo => _bulletAmmo;

    private void Awake()
    {
        _remainingBulletAmmo = _bulletAmmo;
        _animator = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
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

        if (isHit)
        {
            Debug.Log($"{hitInfo.collider.name}이 총알을 맞았다. 데미지: {_damage}");
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
        flash.SetActive(true);
        yield return new WaitForSeconds(_flashTime);
        flash.SetActive(false);
    }
}
