using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private const float Gravity = -20f;
    private float _yVeclocity = 0f; // 위아래 속도, 매 프레임마다 중력 가속도가 붙는다

    [SerializeField] private float _moveSpeed = 7f;
    [SerializeField] private float _jumpPower = 7f;
    [SerializeField] private CharacterController _characterController;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // 1. 키보드 입력을 받는다
        float h = Input.GetAxisRaw("Horizontal"); // -1, 0, 1
        float v = Input.GetAxisRaw("Vertical"); // -1, 0, 1

        // 2. 키보드 입력에 따라 방향을 구한다 ( 위아래로는 움직이지 않으므로 y = 0)
        var direction = new Vector3(h, 0f, v);
        direction.Normalize();
        direction = transform.TransformDirection(direction);

        // 로컬좌표계 : 한 대상을 중심
        // 월드좌표계 : 세상을 중심

        // 3. 땅에 서있지 않을때만 중력을 누적한다
        if (_characterController.isGrounded == false)
        {
            _yVeclocity += Gravity * Time.deltaTime;
        }

        else
        {
            _yVeclocity = 0f;   // 땅에 서있으면 중력 초기화
        }
        // 4. 점프키를 누르면 && 땅이라면 점프를한다
        if (Input.GetButtonDown("Jump") && _characterController.isGrounded)  // 중재자 패턴 : 유니티가 미리 만들어둔 기능에 연결해서 쓰는 방식
        {
            _yVeclocity = _jumpPower;
        }

        // 5. 속도를 구한다
        var velocity = direction * _moveSpeed;
        velocity.y = _yVeclocity;
        _characterController.Move(velocity * Time.deltaTime);

        // 새 위치 = 현재 위치 + 방향 * 속도 * 시간
        // transform.position += direction * MoveSpeed * Time.deltaTime;
        _characterController.Move(direction * _moveSpeed * Time.deltaTime);
    }
}