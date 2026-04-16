using UnityEngine;

public class SquadFormationPanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Dependencies")]
    [SerializeField] private PlayerSquadBridge bridge;

    [Header("Party Slots")]
    [SerializeField] private SquadFormationPartySlotView partySlot1;
    [SerializeField] private SquadFormationPartySlotView partySlot2;
    [SerializeField] private SquadFormationPartySlotView partySlot3;

    [Header("Roster Slots")]
    [SerializeField] private SquadFormationRosterSlotView[] rosterSlots;

    [Header("Drag")]
    [SerializeField] private CharacterDragVisual dragVisual;

    private CharacterFormationDragPayload currentDragPayload;

    private void Awake()
    {
        HideImmediate();
        BindEvents();
    }

    private void Update()
    {
        if (root == null || !root.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
            {
                Hide();
            }
    }

    private void BindEvents()
    {
        if (partySlot1 != null)
        {
            partySlot1.OnDroppedOn += OnPartySlotDroppedOn;
            partySlot1.OnBeginDragEvent += OnPartyBeginDrag;
            partySlot1.OnEndDragEvent += OnPartyEndDrag;
            partySlot1.OnDoubleClickEvent += OnPartyDoubleClick;
        }

        if (partySlot2 != null)
        {
            partySlot2.OnDroppedOn += OnPartySlotDroppedOn;
            partySlot2.OnBeginDragEvent += OnPartyBeginDrag;
            partySlot2.OnEndDragEvent += OnPartyEndDrag;
            partySlot2.OnDoubleClickEvent += OnPartyDoubleClick;
        }

        if (partySlot3 != null)
        {
            partySlot3.OnDroppedOn += OnPartySlotDroppedOn;
            partySlot3.OnBeginDragEvent += OnPartyBeginDrag;
            partySlot3.OnEndDragEvent += OnPartyEndDrag;
            partySlot3.OnDoubleClickEvent += OnPartyDoubleClick;
        }

        if (rosterSlots != null)
        {
            for (int i = 0; i < rosterSlots.Length; i++)
            {
                if (rosterSlots[i] == null)
                    continue;

                rosterSlots[i].OnBeginDragEvent += OnRosterBeginDrag;
                rosterSlots[i].OnEndDragEvent += OnRosterEndDrag;
                rosterSlots[i].OnDoubleClickEvent += OnRosterDoubleClick;
                rosterSlots[i].OnDroppedOn += OnRosterDroppedOn;
            }
        }
    }

    public void Show()
    {
        if (root != null)
        {
            root.SetActive(true);
            root.transform.SetAsLastSibling();
        }

        if (bridge != null)
            bridge.SetModalBlocker("Formation", true);

        RefreshView();
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        if (bridge != null)
            bridge.SetModalBlocker("Formation", false);

        ClearDragState();
    }
    private void HideImmediate()
    {
        if (root != null)
            root.SetActive(false);

        if (bridge != null)
            bridge.SetFormationPanelOpen(false);

        ClearDragState();
    }

    public void Toggle()
    {
        if (root == null)
            return;

        if (root.activeSelf) Hide();
        else Show();
    }

    public void RefreshView()
    {
        if (bridge == null)
            return;

        if (partySlot1 != null)
            partySlot1.RefreshView(bridge.GetCharacterAtPosition(PositionIndex.Position1));

        if (partySlot2 != null)
            partySlot2.RefreshView(bridge.GetCharacterAtPosition(PositionIndex.Position2));

        if (partySlot3 != null)
            partySlot3.RefreshView(bridge.GetCharacterAtPosition(PositionIndex.Position3));

        if (rosterSlots != null)
        {
            for (int i = 0; i < rosterSlots.Length; i++)
            {
                if (rosterSlots[i] != null)
                    rosterSlots[i].RefreshView();
            }
        }
    }

    private void OnRosterBeginDrag(SquadFormationRosterSlotView slotView)
    {
        if (slotView == null || slotView.CharacterDefinition == null)
            return;

        currentDragPayload = new CharacterFormationDragPayload(
            FormationDragSourceType.Roster,
            slotView.CharacterDefinition,
            PositionIndex.None
        );

        if (dragVisual != null)
            dragVisual.Show(slotView.CharacterDefinition);
    }

    private void OnRosterEndDrag(SquadFormationRosterSlotView slotView)
    {
        ClearDragState();
    }

    private void OnPartyBeginDrag(SquadFormationPartySlotView slotView)
    {
        if (slotView == null || slotView.CurrentCharacter == null)
            return;

        currentDragPayload = new CharacterFormationDragPayload(
            FormationDragSourceType.Party,
            slotView.CurrentCharacter,
            slotView.PositionIndex
        );

        if (dragVisual != null)
            dragVisual.Show(slotView.CurrentCharacter);
    }

    private void OnPartyEndDrag(SquadFormationPartySlotView slotView)
    {
        ClearDragState();
    }

    private void OnPartySlotDroppedOn(SquadFormationPartySlotView targetSlot)
    {
        if (bridge == null || currentDragPayload == null || currentDragPayload.CharacterDefinition == null || targetSlot == null)
        {
            ClearDragState();
            return;
        }

        bool success = false;

        // 로스터 -> 파티
        if (currentDragPayload.SourceType == FormationDragSourceType.Roster)
        {
            success = bridge.SetCharacterToPosition(targetSlot.PositionIndex, currentDragPayload.CharacterDefinition);
        }
        // 파티 -> 파티
        else if (currentDragPayload.SourceType == FormationDragSourceType.Party)
        {
            if (currentDragPayload.SourcePositionIndex == targetSlot.PositionIndex)
            {
                success = true;
            }
            else
            {
                success = bridge.SwapCharactersBetweenPositions(
                    currentDragPayload.SourcePositionIndex,
                    targetSlot.PositionIndex
                );
            }
        }

        Debug.Log($"[SquadFormationPanelUI] 파티 슬롯 드롭 결과: {success}");

        ClearDragState();
        RefreshView();
    }

    private void OnRosterDroppedOn(SquadFormationRosterSlotView targetRosterSlot)
    {
        if (bridge == null || currentDragPayload == null)
        {
            ClearDragState();
            return;
        }

        bool success = false;

        // 파티 -> 로스터 = 해제
        if (currentDragPayload.SourceType == FormationDragSourceType.Party)
        {
            success = bridge.ClearCharacterAtPosition(currentDragPayload.SourcePositionIndex);
        }

        Debug.Log($"[SquadFormationPanelUI] 로스터 슬롯 드롭 결과: {success}");

        ClearDragState();
        RefreshView();
    }

    private void OnRosterDoubleClick(SquadFormationRosterSlotView slotView)
    {
        if (bridge == null || slotView == null || slotView.CharacterDefinition == null)
            return;

        if (TryAssignToFirstEmptySlot(slotView.CharacterDefinition))
        {
            RefreshView();
        }
    }

    private void OnPartyDoubleClick(SquadFormationPartySlotView slotView)
    {
        if (bridge == null || slotView == null)
            return;

        bool success = bridge.ClearCharacterAtPosition(slotView.PositionIndex);
        Debug.Log($"[SquadFormationPanelUI] 파티 슬롯 더블클릭 해제 결과: {success}");

        RefreshView();
    }

    private bool TryAssignToFirstEmptySlot(CharacterDefinition character)
    {
        if (bridge.GetCharacterAtPosition(PositionIndex.Position1) == null)
            return bridge.SetCharacterToPosition(PositionIndex.Position1, character);

        if (bridge.GetCharacterAtPosition(PositionIndex.Position2) == null)
            return bridge.SetCharacterToPosition(PositionIndex.Position2, character);

        if (bridge.GetCharacterAtPosition(PositionIndex.Position3) == null)
            return bridge.SetCharacterToPosition(PositionIndex.Position3, character);

        return false;
    }

    private void ClearDragState()
    {
        currentDragPayload = null;

        if (dragVisual != null)
            dragVisual.Hide();
    }
}