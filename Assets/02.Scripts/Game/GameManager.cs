using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.SceneManagement;

public enum GameState
{
    Ready,
    Pause,
    Playing,
    GameOver
}

public class GameManager : SingletonBehaviour<GameManager>
{
    private GameState _state = GameState.Ready;
    public GameState State => _state;

    [SerializeField] private TextMeshProUGUI _stateText;
    [SerializeField] private Animator _playerAnimator;
    [SerializeField] private GameObject _optionPopup;
    [SerializeField] private GameObject _gameOverPopup;

    private void Start()
    {
        _optionPopup.SetActive(false);
        StartCoroutine(StartSequence_Coroutine());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_state == GameState.Playing)
                Pause();
            else if (_state == GameState.Pause) Resume();
        }
    }

    private IEnumerator StartSequence_Coroutine()
    {
        _stateText.gameObject.SetActive(true);
        _stateText.text = "준비...";
        yield return new WaitForSeconds(1.5f);
        _stateText.text = "시작!";
        yield return new WaitForSeconds(0.5f);
        _stateText.gameObject.SetActive(false);
        _state = GameState.Playing;
    }

    public void GameOver()
    {
        _state = GameState.GameOver;
        _stateText.gameObject.SetActive(false);
        CursorManager.Instance.UnlockCursor();
        _gameOverPopup.SetActive(true);
        _playerAnimator.SetFloat("MoveMotion", 0f);
    }

    public void Pause()
    {
        _state = GameState.Pause;
        Time.timeScale = 0;
        _optionPopup.SetActive(true);
        CursorManager.Instance.UnlockCursor();
    }

    public void Resume()
    {
        _state = GameState.Playing;
        Time.timeScale = 1;
        _optionPopup.SetActive(false);
        CursorManager.Instance.LockCursor();
    }

    public void Restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}