using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;

    [SerializeField] private Player _player;
    [SerializeField] private PlayerMove _playerMove;

    private void Start()
    {
        _player.OnDamaged += RefreshHealth;
    }

    private void Update()
    {
        // RefreshHealth();
        _staminaSlider.value = _playerMove.Stamina / _playerMove.MaxStaminaValue;
    }

    private void RefreshHealth()
    {
        // 옵저버패턴 :한 객체(주체)의 상태가 바뀌면 그것을 구독한 다른 객체들에게 자동으로 알려주는 설계 패턴으로, 서로 직접 의존하지 않고 이벤트로 느슨하게 연결됩니다.
        _healthSlider.value = _player.Health / _player.MaxHealth;
    }
}