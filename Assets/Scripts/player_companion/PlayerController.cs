using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float defaultMoveSpeed = 5f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.8f;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;
    private Vector3 dashDirection;
    private float currentMoveSpeed;
    private CharacterController characterController;
    private PlayerSquadBridge bridge;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        currentMoveSpeed = defaultMoveSpeed;
    }
    public void SetBridge(PlayerSquadBridge squadBridge)
    {
        bridge = squadBridge;
    }


    // Update is called once per frame
    void Update()
    {
        if (bridge != null && !bridge.CanPlayerMove())
            return;
        UpdateDashCooldown();
        HandleDash();

        if (isDashing)
        {
            DashMove();
        }
        else
        {
            Move();
        }
        RotateToMouse();
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal"); // 조작키 A, D 에 대해 정의 
        float v = Input.GetAxisRaw("Vertical");   // 조작키 W, S 에 대해 정의

        Vector3 move = new Vector3(h, 0, v);
        if (move.magnitude > 1f)
        {
            move.Normalize();
        }

        characterController.Move(move * currentMoveSpeed * Time.deltaTime); //캐컨으로 이동 제어
    }

    void RotateToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); //카메라가 마우스위치에 ray를 쏨
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); //y =0 위치에 있는 평평한 바닥을 하나 만들겠다.Vector3.zero =0,0,0

        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance)) //ray가 groundplane과 만나면true반환, 카메라에서부터 바닥까지 ray가 간 거리를 rayDistance에 넣음
        {
            Vector3 targetPoint = ray.GetPoint(rayDistance); //ray를 rayDistance만큼 따라갔을때의 실제 위치를 구하라> 마우스의 3d 위치를 찾음

            Vector3 lookDirection = targetPoint - transform.position; //플레이어위치 에서 마우스위치 까지의 방향벡터를 계산
            lookDirection.y = 0f;  //수평 방향만 바라보게 함(탑뷰니까)

            if (lookDirection != Vector3.zero) //방향 벡터가 0인 경우는 제외 변수발생 차단
            {
                transform.rotation = Quaternion.LookRotation(lookDirection); /*Quaternion.LookRotation(방향벡터)은 방향을 바라보게 하는 회전값을 만드는 함수
                이 회전값을 현 하이라키?(=플레이어)의transform의 rotation에 적용한다. */
            }
        }

    }

     void HandleDash()
    {
        if (isDashing) return;

        if (dashCooldownTimer > 0f) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            Vector3 inputDirection = new Vector3(h, 0, v).normalized;

            if (inputDirection != Vector3.zero)
            {
                dashDirection = inputDirection; //우선 조작키 입력 방향 대시
            }
            else
            {
                dashDirection = transform.forward; //바라보는 방향 대시
                dashDirection.y = 0f;
                dashDirection.Normalize();
            }

            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;
        }
    }

    void DashMove()
    {
        characterController.Move(dashDirection * dashSpeed * Time.deltaTime);

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;
        }
    }

    void UpdateDashCooldown()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
    public bool IsDashing() //PlayerShooter에서 대시 중 사격제한을 위한 함수
    {
        return isDashing;
    }

    public void SetMoveSpeed(float newMoveSpeed)
    {
        // 0 이하 방지
        currentMoveSpeed = Mathf.Max(0f, newMoveSpeed);
    }
    public float GetCurrentMoveSpeed() //디버그용
    {
        return currentMoveSpeed;
    }
    public void ResetRuntimeStateAfterRespawn()
    {
        isDashing = false;
        dashTimer = 0f;
        dashCooldownTimer = 0f;
        dashDirection = Vector3.zero;
    }
}
