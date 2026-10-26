using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UI_Minimap : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private TextMeshProUGUI _mapNameText;
    [SerializeField] private float _zoomStep = 2f;
    [SerializeField] private float _minSize = 4f;
    [SerializeField] private float _maxSize = 16f;

    public void OnClickPlus()
    {
        ZoomIn();
    }

    public void OnClickMinus()
    {
        ZoomOut();
    }

    private void ZoomIn()
    {
        float size = _camera.orthographicSize - _zoomStep;
        _camera.orthographicSize = Mathf.Max(size, _minSize);
    }
    private void ZoomOut()
    {
        float size = _camera.orthographicSize + _zoomStep;
        _camera.orthographicSize = Mathf.Min(size, _maxSize);
    }
}
