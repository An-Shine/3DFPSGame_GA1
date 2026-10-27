using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private const float Gravity = -20f;
    private const int MaxJumps = 2;
    private const float MaxStamina = 100f;

    private float _yVeclocity = 0f; // 위아래 속도, 매 프레임마다 중력 가속도가 붙음
    private int _jumpCount = 0;
    private float _stamina = 0f;

    [Header("스탯")]
    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _sprintSpeed = 12f;
    [SerializeField] private float _jumpPower = 7f;
    [SerializeField] private float _staminaRecovery = 15f;
    [SerializeField] private float _sprintCost = 20f;
    [SerializeField] private float _jumpCost = 20f;

    [Header("컴포넌트")]
    [SerializeField] private CharacterController _characterController;

    public float Stamina => _stamina;
    public float MaxStaminaValue => MaxStamina;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _stamina = MaxStamina;
    }

    private void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;
        // 1. 키보드 입력을 받음
        float h = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        float v = Input.GetAxisRaw("Vertical"); // -1, 0, 1

        // 2. 키보드 입력에 따라 방향을 구함 ( 위아래로는 움직이지 않으므로 y = 0)
        Vector3 direction = new(h, 0f, v);
        direction.Normalize();
        direction = transform.TransformDirection(direction);

        // 로컬좌표계 : 한 대상을 중심
        // 월드좌표계 : 세상을 중심

        // 3. 땅에 서있지 않을때만 중력을 누적
        if (_characterController.isGrounded == false)
        {
            _yVeclocity += Gravity * Time.deltaTime;
        }
        else
        {
            _yVeclocity = -1f; // 땅에 서있으면 중력 초기화
            _jumpCount = 0;
        }

        // 4. 점프키를 누르면 && 점프카운트가 MaxJump 이하라면 점프를 함
        bool isJumped = false;
        if (Input.GetButtonDown("Jump") && _jumpCount < MaxJumps && _stamina >= _jumpCost) // 중재자 패턴 : 유니티가 미리 만들어둔 기능에 연결해서 쓰는 방식
        {
            _yVeclocity = _jumpPower;
            _jumpCount++;
            _stamina -= _jumpCost;
            isJumped = true;
        }

        // 5. 속도를 구함
        bool wantsSprint = Input.GetKey(KeyCode.LeftShift) && direction.sqrMagnitude > 0f;
        bool isSprinting = wantsSprint && _stamina > 0f;
        float currentSpeed = _moveSpeed;

        if (isSprinting)
        {
            currentSpeed = _sprintSpeed;
            _stamina -= _sprintCost * Time.deltaTime;
            if (_stamina < _sprintCost)
            {
                currentSpeed = _moveSpeed;
                isSprinting = false;
            }
        }
        else if (!wantsSprint && !isJumped)
        {
            _stamina += _staminaRecovery * Time.deltaTime;
        }

        // 스태미나가 0보다 작아지거나 최댓값을 넘지 않게 함
        _stamina = Mathf.Clamp(_stamina, 0f, MaxStamina);
        Vector3 velocity = direction * currentSpeed;
        velocity.y = _yVeclocity;
        _characterController.Move(velocity * Time.deltaTime);
    }
}