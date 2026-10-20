using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private const float Gravity = -20f;
    private float _yVeclocity = 0f; // 위아래 속도, 매 프레임마다 중력 가속도가 붙는다

    [SerializeField] private float _moveSpeed = 7f;
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

        // 3. 중력을 누적한다
        _yVeclocity += Gravity * Time.deltaTime;

        // 4. 속도를 구한다
        var velocity = direction * _moveSpeed;
        velocity.y = _yVeclocity;
        _characterController.Move(velocity * Time.deltaTime);

        // 새 위치 = 현재 위치 + 방향 * 속도 * 시간
        // transform.position += direction * MoveSpeed * Time.deltaTime;
        _characterController.Move(direction * _moveSpeed * Time.deltaTime);
    }
}