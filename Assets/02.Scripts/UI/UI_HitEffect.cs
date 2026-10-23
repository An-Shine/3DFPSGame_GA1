using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Image))]
public class UI_HitEffect : MonoBehaviour
{
    [SerializeField] private float _flashTime = 0.2f;
    [SerializeField] private float _maxAlpha = 0.4f;
    [SerializeField] private float _fadeTime = 0.3f;

    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.enabled = false;
    }

    public void Flash(float alpha)
    {
        SetAlpha(alpha);
        StartCoroutine(Flash_Coroutine());
    }

    private IEnumerator Flash_Coroutine()
    {
        _image.enabled = true;

        yield return new WaitForSeconds(_flashTime);
    }
    public void Fade(float alpha)
    {
        _image.enabled = true;
        SetAlpha(alpha);

        StartCoroutine(Fade_Coroutine());
    }
    private IEnumerator Fade_Coroutine()
    {
        float elapsed = 0f;
        while (elapsed < _fadeTime)
        {
            SetAlpha(Mathf.Lerp(_maxAlpha, 0f, elapsed / _fadeTime));
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        Color color = _image.color;
        color.a = alpha;
        _image.color = color;
    }
}