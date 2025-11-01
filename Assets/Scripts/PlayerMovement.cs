using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 앞뒤 움직임의 속도
    public float animationDampTime = 0.1f;
    public float rotateSpeed = 180f; // 좌우 회전 속도
    public LayerMask groundLayerMask;
    private float maxDistance = 100f;
    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디
    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터
    private Camera mainCamera;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨
    private void FixedUpdate()
    {
        // 물리 갱신 주기마다 움직임, 회전, 애니메이션 처리 실행
        Rotate();
        Move();

        Vector3 characterForward = transform.forward;
        Vector3 characterRight = transform.right;

        Vector3 worldMoveDir = playerInput.moveDir.normalized;

        // Vector3.Dot (내적)을 사용해 월드 이동 방향을 캐릭터 기준의 Z축(앞뒤) 값으로 변환
        // worldMoveDir가 characterForward와 같은 방향이면 1, 반대면 -1, 수직이면 0이 나옴
        float localMoveZ = Vector3.Dot(worldMoveDir, characterForward);

        // 월드 이동 방향을 캐릭터 기준의 X축(좌우) 값으로 변환
        // worldMoveDir가 characterRight와 같은 방향이면 1, 반대면 -1, 수직이면 0이 나옴
        float localMoveX = Vector3.Dot(worldMoveDir, characterRight);

        playerAnimator.SetFloat("MoveX", localMoveX, animationDampTime, Time.deltaTime);
        playerAnimator.SetFloat("MoveZ", localMoveZ, animationDampTime, Time.deltaTime); // "MoveZ"가 맞다고 가정

    }

    // 입력값에 따라 캐릭터를 앞뒤로 움직임
    private void Move()
    {
        Vector3 camForward = mainCamera.transform.forward;        
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 dir = camForward * playerInput.inputDir.z + camRight * playerInput.inputDir.x;
        playerInput.moveDir = dir.normalized;
        playerRigidbody.MovePosition(playerRigidbody.position + playerInput.moveDir * moveSpeed * Time.deltaTime);
    }

    // 입력값에 따라 캐릭터를 좌우로 회전
    private void Rotate()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, groundLayerMask) == true)
        {
            var targetPoint = hit.point;
            targetPoint.y = transform.position.y;
            transform.LookAt(targetPoint);
        }
    }
}