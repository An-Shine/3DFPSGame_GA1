using UnityEngine;
using TMPro;

public class UI_Minimap : MonoBehaviour
{
    [SerializeField] private MinimapCamera _minimapCamera;
    [SerializeField] private TextMeshProUGUI _mapNameText;

    public void OnClickPlus()
    {
        _minimapCamera.ZoomIn();
    }

    public void OnClickMinus()
    {
        _minimapCamera.ZoomOut();
    }
}
