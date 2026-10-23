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

    private void LateUpdate()
    {
        _slider.value = _zombie.Health / _zombie.MaxHealth;

        // 빌보드 : 체력바가 카메라를 쳐다보게한다
        _healthBarCanvas.forward = Camera.main.transform.forward;
    }
}