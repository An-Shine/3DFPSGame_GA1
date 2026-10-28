using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_Minimap : MonoBehaviour
{
    [SerializeField] private MinimapCamera _minimapCamera;
    [SerializeField] private TextMeshProUGUI _mapNameText;

    private void Awake()
    {
        _mapNameText.text = SceneManager.GetActiveScene().name;
    }

    public void OnClickPlus()
    {
        _minimapCamera.ZoomIn();
    }

    public void OnClickMinus()
    {
        _minimapCamera.ZoomOut();
    }
}
