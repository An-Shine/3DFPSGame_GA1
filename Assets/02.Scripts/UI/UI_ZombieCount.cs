using UnityEngine;
using TMPro;

public class UI_ZombieCount : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _zombieCountUIText;
    [SerializeField] private ZombieSpawner _spawner;

    private void Start()
    {
        _spawner.OnCountChanged += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        _zombieCountUIText.text = $"{_spawner.AliveCount} / {_spawner.MaxSpawnCount}";
    }
}
