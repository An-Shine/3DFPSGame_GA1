using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UI_Crosshair : MonoBehaviour
{
    private CameraFollow _cameraFollow;
    private RectTransform _rectTransform;
    private RectTransform _parent;
    private Canvas _canvas;
    private Image _image;
    private Vector2 _centerPosition;

    private void Awake()
    {
        _cameraFollow = Camera.main.GetComponent<CameraFollow>();
        _rectTransform = GetComponent<RectTransform>();
        _parent = (RectTransform)_rectTransform.parent;
        _canvas = GetComponentInParent<Canvas>();
        _image = GetComponent<Image>();
        _centerPosition = _rectTransform.anchoredPosition;

        // 마우스 바로 아래의 조준선 이미지가 UI로 판정되어 사격을 막지 않게 한다.
        _image.raycastTarget = false;
    }

    private void LateUpdate()
    {
        if (_cameraFollow.Mode != CameraMode.QuarterView)
        {
            _rectTransform.anchoredPosition = _centerPosition;
            _image.enabled = true;
            return;
        }

        // 일시정지나 Alt 조작 중에는 일반 커서를 보여 주고 조준선은 숨긴다.
        _image.enabled = GameManager.Instance.State == GameState.Playing && !Cursor.visible;
        Camera uiCamera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _canvas.worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parent, Input.mousePosition, uiCamera, out Vector2 position))
        {
            _rectTransform.localPosition = new Vector3(position.x, position.y, _rectTransform.localPosition.z);
        }
    }
}
