using UnityEngine;
using UnityEngine.UI;

public class UI_PlayerStats : MonoBehaviour
{
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Slider _staminaSlider;

    [SerializeField] private Player _player;
    [SerializeField] private PlayerMove _playerMove;

    private void LateUpdate()
    {
        _healthSlider.value = _player.Health / _player.MaxHealth;
        _staminaSlider.value = _playerMove.Stamina / _playerMove.MaxStaminaValue;
    }
}