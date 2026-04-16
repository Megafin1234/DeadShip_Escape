using UnityEngine;

/// <summary>
/// 숫자키 1~4 입력으로 퀵슬롯 아이템 사용.
/// 
/// 나중에 확장 가능:
/// - 마우스 휠로 선택
/// - 선택된 퀵슬롯 강조
/// - 클릭 사용
/// - 패드 입력
/// </summary>
public class QuickSlotInputController : MonoBehaviour
{
    [SerializeField] private PlayerSquadBridge bridge;

    private void Update()
    {
        if (bridge == null)
            return;

        // 이동/사격처럼 완전히 막을지,
        // UI 열려도 퀵슬롯은 허용할지 정책을 바꿀 수 있음.
        // 지금은 다른 행동과 동일하게 CanPlayerAct 기준을 따름.
        if (!bridge.CanPlayerAct())
            return;

        HandleQuickSlotInput();
    }

    private void HandleQuickSlotInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UseQuickSlot(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            UseQuickSlot(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            UseQuickSlot(2);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            UseQuickSlot(3);
    }

    private void UseQuickSlot(int index)
    {
        ItemMoveResult result = bridge.TryUseQuickSlot(index);

        if (result.Success)
            Debug.Log($"[QuickSlotInput] {index + 1}번 슬롯 사용 성공: {result.Message}");
        else
            Debug.LogWarning($"[QuickSlotInput] {index + 1}번 슬롯 사용 실패: {result.Message}");
    }
}