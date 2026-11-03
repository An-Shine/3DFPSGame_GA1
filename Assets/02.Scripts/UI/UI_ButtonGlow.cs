using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 버튼에 마우스를 올리면 Glow 이미지가 서서히 켜지고, 켜진 동안 살짝 맥동한다.
public class UI_ButtonGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _glow;               // 버튼 뒤에서 빛나는 Glow 이미지
    [SerializeField] private float _fadeInTime = 0.3f;  // 켜지는 데 걸리는 시간(초)
    [SerializeField] private float _fadeOutTime = 0.45f; // 꺼지는 데 걸리는 시간(초), 느리게 꺼질수록 부드럽다
    [SerializeField] private float _pulseSpeed = 4f;    // 맥동 속도
    [SerializeField, Range(0f, 1f)] private float _pulseAmount = 0.2f; // 맥동 크기(최대 알파 대비 비율)

    // 켜졌을 때의 최대 알파. Glow 이미지 자체는 알파 0으로 두어 에디터에서 버튼과 겹쳐 보이지 않게 한다
    [SerializeField, Range(0f, 1f)] private float _maxAlpha = 0.8f;

    private float _weight; // 0 = 꺼짐, 1 = 켜짐
    private bool _isHover;
    private Selectable _selectable;

    private void Awake()
    {
        _selectable = GetComponent<Selectable>();
        SetAlpha(0f);
    }

    // 비활성화됐다가 다시 켜질 때 켜진 채로 남지 않도록 초기화
    private void OnDisable()
    {
        _isHover = false;
        _weight = 0f;
        if (_glow != null)
        {
            SetAlpha(0f);
        }
    }

    private void Update()
    {
        bool on = _isHover && (_selectable == null || _selectable.interactable);
        float fadeTime = on ? _fadeInTime : _fadeOutTime;
        _weight = Mathf.MoveTowards(_weight, on ? 1f : 0f, Time.unscaledDeltaTime / fadeTime);

        // SmoothStep으로 시작과 끝을 완만하게 만들어 뚝뚝 끊기는 느낌을 없앤다
        float eased = Mathf.SmoothStep(0f, 1f, _weight);
        float pulse = 1f - _pulseAmount * (0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * _pulseSpeed));
        SetAlpha(_maxAlpha * eased * Mathf.Lerp(1f, pulse, eased));
    }

    public void OnPointerEnter(PointerEventData eventData) => _isHover = true;

    public void OnPointerExit(PointerEventData eventData) => _isHover = false;

    private void SetAlpha(float alpha)
    {
        Color c = _glow.color;
        c.a = alpha;
        _glow.color = c;
    }
}
