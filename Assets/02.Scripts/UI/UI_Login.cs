using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 로그인 화면. 로그인 모드와 회원가입 모드를 바꾸고, 입력을 확인해 AccountStorage에 부탁한다.
public class UI_Login : MonoBehaviour
{
    private enum Mode
    {
        Login, // 로그인
        Register, // 회원가입
    }

    [Header("입력 칸")]
    [SerializeField] private TMP_InputField _idInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private TMP_InputField _passwordConfirmInput; // 회원가입 모드에서만 보인다.

    [Header("버튼")]
    [SerializeField] private Button _loginButton; // 로그인 모드: 로그인
    [SerializeField] private Button _gotoRegisterButton; // 로그인모드 > 회원가입모드로
    [SerializeField] private Button _registerButton; //  회원가입버튼
    [SerializeField] private Button _gotoLoginButton; //  회원가입모드 > 로그인모드로

    [Header("기타")]
    [SerializeField] private TextMeshProUGUI _messageText; // 안내 글자
    [SerializeField] private string _gameSceneName = "GameScene"; // 로그인하면 불러올 씬

    private Mode _mode = Mode.Login;

    private PlayerPrefsAccountRepository _repository = new PlayerPrefsAccountRepository();
    private AccountIDSpecification _idSpecification = new AccountIDSpecification();
    private AccountPasswordSpecification _passwordSpecification = new AccountPasswordSpecification();

    private void Start()
    {
        Refresh();
    }

    private void Refresh()
    {
        bool isLogin = _mode == Mode.Login;
        _passwordConfirmInput.gameObject.SetActive(!isLogin);
        _loginButton.gameObject.SetActive(isLogin);
        _gotoRegisterButton.gameObject.SetActive(isLogin);
        _registerButton.gameObject.SetActive(!isLogin);
        _gotoLoginButton.gameObject.SetActive(!isLogin);
    }

    public void GotoLogin()
    {
        _mode = Mode.Login;
        _messageText.text = string.Empty;
        Refresh();
    }

    public void GotoRegister()
    {
        _mode = Mode.Register;
        _messageText.text = string.Empty;
        Refresh();
    }

    public void Login()
    {
        string id = _idInput.text;
        string password = _passwordInput.text;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해 주세요.";
            return;
        }

        if (_repository.CheckPassword(id, password))
        {
            SceneLoader.Load("GameScene");
        }
        else
        {
            _messageText.text = "아이디와 비밀번호를 확인해주세요";
        }
    }

    public void Register()
    {
        string id = _idInput.text;
        string password = _passwordInput.text;
        string passwordConfirm = _passwordConfirmInput.text;

        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
        {
            _messageText.text = "아이디와 비밀번호를 입력해주세요.";
            return;
        }

        // 아이디 규칙(이메일 형식)은 AccountIDSpecification에서 검사한다.
        if (!_idSpecification.IsSatisfiedBy(id))
        {
            _messageText.text = _idSpecification.ErrorMessage;
            return;
        }

        // 비밀번호 규칙은 AccountPasswordSpecification에서 검사한다.
        if (!_passwordSpecification.IsSatisfiedBy(password))
        {
            _messageText.text = _passwordSpecification.ErrorMessage;
            return;
        }

        if (password != passwordConfirm)
        {
            _messageText.text = "비밀번호 확인이 맞지 않습니다.";
            return;
        }

        if (_repository.Exists(id))
        {
            _messageText.text = "이미 존재하는 아이디 입니다.";
            return;
        }

        _repository.Register(id, password);
        GotoLogin();
        _messageText.text = "가입했습니다. 로그인해주세요";
    }
}