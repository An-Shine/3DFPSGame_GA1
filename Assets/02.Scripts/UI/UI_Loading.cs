using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Loading : MonoBehaviour
{
    [Header("로딩 UI")]
    [SerializeField] private Slider _progressSlider;
    [SerializeField] private TextMeshProUGUI _progressText;

    [Header("연출")]
    [SerializeField] private float _fillSpeed = 0.5f; // 초당 채워지는 비율. 로딩이 빨라도 최소 1/_fillSpeed 초는 보여준다.

    private void Start()
    {
        StartCoroutine(Load_Coroutine());
    }

    private IEnumerator Load_Coroutine()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        // 비동기 씬 로드
        // 반환 : 비동기 진행사항을 확인할 수 있는 객체
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneLoader.NextSceneName);
        // 수많은 객체의 생성을 쪼개서 하나하나 생성할 준비를한다
        operation.allowSceneActivation = false;

        float displayValue = 0f; // 화면에 보여주는 값(실제 진행도를 따라가며 부드럽게 증가)
        SetProgress(displayValue);

        while (displayValue < 1f)
        {
            // 활성화 대기 중에는 progress가 0.9에서 멈추므로 0~1로 보간한다.
            float target = Mathf.Clamp01(operation.progress / 0.9f);
            displayValue = Mathf.MoveTowards(displayValue, target, _fillSpeed * Time.deltaTime);
            SetProgress(displayValue);
            yield return null;
        }

        // 표시가 100%에 도달한 뒤에 씬을 활성화한다.
        operation.allowSceneActivation = true;
    }

    private void SetProgress(float value)
    {
        _progressSlider.value = value;
        _progressText.text = $"불러오는 중 {Mathf.RoundToInt(value * 100)}%";
    }
}
