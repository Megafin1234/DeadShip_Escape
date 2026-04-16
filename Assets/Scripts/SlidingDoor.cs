using UnityEngine;

public class SlidingDoor : MonoBehaviour
{
    public enum DoorState
    {
        Closed,
        Opening,
        Open,
        Closing
    }

    [Header("Panels")]
    [SerializeField] private Transform leftPanel; //왼쪽문
    [SerializeField] private Transform rightPanel; //오른쪽문

    [Header("Movement")]
    [SerializeField] private Vector3 leftOpenOffset = new Vector3(-1.2f, 0f, 0f); //닫힌 위치 기준 왼쪽 문이 열릴때까지 이동하는 거리
    [SerializeField] private Vector3 rightOpenOffset = new Vector3(1.2f, 0f, 0f); //       ''     오른쪽
    [SerializeField] private float moveSpeed = 3f; //문열리는속도

    [Header("Blocking")]
    [SerializeField] private Collider blockingCollider; //실제 닫힌문 상태에서 문을 막는 역할 문의 콜라이더나 메시랑 달리 길막을 독립적으로 제어하기 위함

    private Vector3 leftClosedLocalPos;
    private Vector3 rightClosedLocalPos;
    private Vector3 leftOpenLocalPos;
    private Vector3 rightOpenLocalPos;

    public DoorState CurrentState { get; private set; } = DoorState.Closed;

    private void Start() //문의 닫힌상태 위치 계산
    {
        if (leftPanel != null)
            leftClosedLocalPos = leftPanel.localPosition;

        if (rightPanel != null)
            rightClosedLocalPos = rightPanel.localPosition;

        leftOpenLocalPos = leftClosedLocalPos + leftOpenOffset;
        rightOpenLocalPos = rightClosedLocalPos + rightOpenOffset;

        if (blockingCollider != null)
            blockingCollider.enabled = (CurrentState == DoorState.Closed);
    }

    private void Update()
    {
        switch (CurrentState)
        {
            case DoorState.Opening:
                UpdateOpening();
                break;

            case DoorState.Closing:
                UpdateClosing();
                break;
        }
    }

    private void UpdateOpening()
    {
        bool leftDone = MovePanel(leftPanel, leftOpenLocalPos);
        bool rightDone = MovePanel(rightPanel, rightOpenLocalPos);

        if (leftDone && rightDone)
        {
            CurrentState = DoorState.Open;
        }
    }

    private void UpdateClosing()
    {
        bool leftDone = MovePanel(leftPanel, leftClosedLocalPos);
        bool rightDone = MovePanel(rightPanel, rightClosedLocalPos);

        if (leftDone && rightDone)
        {
            CurrentState = DoorState.Closed;

            if (blockingCollider != null)
                blockingCollider.enabled = true;
        }
    }

    private bool MovePanel(Transform panel, Vector3 targetLocalPos) //문짝이동
    {
        if (panel == null)
            return true;

        panel.localPosition = Vector3.MoveTowards(
            panel.localPosition,
            targetLocalPos,
            moveSpeed * Time.deltaTime
        );

        return Vector3.Distance(panel.localPosition, targetLocalPos) <= 0.01f;
    }

    public bool IsBusy() // 문의 상호작용 잠금상태 여부
    {
        return CurrentState == DoorState.Opening || CurrentState == DoorState.Closing;
    }

    public bool IsOpen()
    {
        return CurrentState == DoorState.Open;
    }

    public float GetTransitionDuration() // 상호작용 잠금시간을 맞추기 위함. 즉 문짝이 다 열려야 상호작용잠금해제를 하기 위함
    {
        float leftDistance = leftPanel != null ? leftOpenOffset.magnitude : 0f;
        float rightDistance = rightPanel != null ? rightOpenOffset.magnitude : 0f;
        float maxDistance = Mathf.Max(leftDistance, rightDistance);

        if (moveSpeed <= 0f)
            return 0f;

        return maxDistance / moveSpeed;
    }

    public void OpenDoor()
    {
        if (CurrentState == DoorState.Open || CurrentState == DoorState.Opening)
            return;
        if (blockingCollider != null)
        blockingCollider.enabled = false;

        CurrentState = DoorState.Opening;
    }

    public void CloseDoor()
    {
        if (CurrentState == DoorState.Closed || CurrentState == DoorState.Closing)
            return;

        CurrentState = DoorState.Closing;
    }

    public void ToggleDoor() //여는중 닫기 등을 막기 위한 토글방지
    {
        if (IsBusy())
            return;

        if (CurrentState == DoorState.Closed)
            OpenDoor();
        else if (CurrentState == DoorState.Open)
            CloseDoor();
    }
}