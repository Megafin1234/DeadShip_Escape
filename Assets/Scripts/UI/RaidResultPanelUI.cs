using TMPro;
using UnityEngine;

public class RaidResultPanelUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text messageText;

    [Header("Detail Texts")]
    [SerializeField] private TMP_Text GainedSummaryText;
    [SerializeField] private TMP_Text lostSummaryText;

    [Header("Options")]
    [SerializeField] private bool autoHide = false;
    [SerializeField] private float autoHideDelay = 2.5f;

    [Header("Dependencies")]
    [SerializeField] private PlayerSquadBridge bridge;

    private float hideTimer = 0f;
    private bool isShowing = false;

    private void Awake()
    {
        HideImmediate();
    }

    private void Update()
    {
        if (!isShowing || !autoHide)
            return;

        hideTimer -= Time.deltaTime;
        if (hideTimer <= 0f)
        {
            Hide();
        }
    }

    public void Show(string title, string message)
    {
        if (root != null)
            root.SetActive(true);

        if (bridge != null)
        bridge.SetModalBlocker("RaidResult", true);

        if (titleText != null)
            titleText.text = title;

        if (messageText != null)
            messageText.text = message;

        if (GainedSummaryText != null)
            GainedSummaryText.text = "";

        if (lostSummaryText != null)
            lostSummaryText.text = "";

        isShowing = true;
        hideTimer = autoHideDelay;
    }

    public void ShowSummary(RaidResultSummary summary)
    {
        if (summary == null)
        {
            Show("복귀 완료", "결과 정보가 없습니다.");
            return;
        }

        string title;
        string message;

        switch (summary.ResultType)
        {
            case RaidSessionResultType.Extracted:
                title = "탈출 성공";
                message = "레이드에서 무사히 복귀했습니다.";
                break;

            case RaidSessionResultType.Wiped:
                title = "레이드 실패";
                message = "사망으로 인해 일부 아이템을 잃고 복귀했습니다.";
                break;

            default:
                title = "복귀 완료";
                message = "거점으로 돌아왔습니다.";
                break;
        }

        Show(title, message);

        if (GainedSummaryText != null)
        {
            GainedSummaryText.text =
                $"획득 아이템 {summary.GainedItemCount}개\n" +
                FormatItemList(summary.GainedItemNames);
        }

        if (lostSummaryText != null)
        {
            lostSummaryText.text =
                $"손실 아이템 {summary.LostItemCount}개\n" +
                FormatItemList(summary.LostItemNames);
        }
    }

    public void Hide()
    {
        isShowing = false;

        if (root != null)
            root.SetActive(false);

        if (bridge != null)
        bridge.SetModalBlocker("RaidResult", false);
    }

    private void HideImmediate()
    {
        isShowing = false;

        if (root != null)
            root.SetActive(false);
    }

    private string FormatItemList(System.Collections.Generic.List<string> items)
    {
        if (items == null || items.Count == 0)
            return "- 없음";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        int maxLines = Mathf.Min(items.Count, 8);
        for (int i = 0; i < maxLines; i++)
        {
            sb.Append("• ");
            sb.Append(items[i]);
            sb.Append('\n');
        }

        if (items.Count > maxLines)
        {
            sb.Append($"... 외 {items.Count - maxLines}개");
        }

        return sb.ToString().TrimEnd();
    }
}