using UnityEngine;
using UnityEngine.UI;

public class ZombieHealthBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private Transform _healthBarCanvas;

    private Zombie _zombie;

    private void Awake()
    {
        _zombie = GetComponent<Zombie>();
    }

    private void Start()
    {
        if (_zombie != null)
        {
            _zombie.OnDamaged += OnZombieDamaged;
        }
    }

    private void LateUpdate()
    {
        // 빌보드 : 체력바가 카메라를 쳐다보게 함
        _healthBarCanvas.forward = Camera.main.transform.forward;
    }

    private void OnZombieDamaged()
    {
        RefreshHealthBar();
    }

    private void RefreshHealthBar()
    {
        _slider.value = _zombie.Health / _zombie.MaxHealth;
    }
}