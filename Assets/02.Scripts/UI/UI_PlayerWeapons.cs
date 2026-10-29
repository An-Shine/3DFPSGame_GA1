using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_PlayerWeapons : MonoBehaviour
{
    [SerializeField] private Image _weaponImages;
    [SerializeField] private Sprite[] _modeImages;
    [SerializeField] private TextMeshProUGUI _bulletCountText;
    [SerializeField] private TextMeshProUGUI _bombText;

    [SerializeField] private Player _player;
    [SerializeField] private PlayerGunFire _playerGunFire;
    [SerializeField] private PlayerBombFire _playerBombFire;

    private void Start()
    {
        RefreshIcon(_player.WeaponMode);
        RefreshBullet(_playerGunFire.RemainingBulletAmmo, _playerGunFire.BulletAmmo);
        RefreshBomb(_playerBombFire.BombCount);
    }

    private void OnEnable()
    {
        _player.OnModeChanged += RefreshIcon;
        _playerGunFire.OnBulletCountChanged += RefreshBullet;
        _playerBombFire.OnBombCountChanged += RefreshBomb;
    }

    private void OnDisable()
    {
        _player.OnModeChanged -= RefreshIcon;
        _playerGunFire.OnBulletCountChanged -= RefreshBullet;
        _playerBombFire.OnBombCountChanged -= RefreshBomb;
    }

    private void RefreshIcon(WeaponMode mode)
    {
        _weaponImages.sprite = _modeImages[(int)mode];
    }

    private void RefreshBullet(int count, int bulletCount)
    {
        _bulletCountText.text = $"{count}/{bulletCount}";
    }

    private void RefreshBomb(int count)
    {
        _bombText.text = $"남은 폭탄 : {count}";
    }
}