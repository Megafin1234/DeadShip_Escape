using System;
using UnityEngine;

public class SessionStateController : MonoBehaviour
{
    public static SessionStateController Instance { get; private set; }

    [Header("Current State")]
    [SerializeField] private SessionAreaType currentArea = SessionAreaType.Base;

    public SessionAreaType CurrentArea => currentArea;

    public bool IsInBase => currentArea == SessionAreaType.Base;
    public bool IsInRaid => currentArea == SessionAreaType.Raid;

    public event Action<SessionAreaType> OnAreaChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SessionStateController] 중복 Instance가 감지되었습니다.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetArea(SessionAreaType nextArea)
    {
        if (currentArea == nextArea)
            return;

        currentArea = nextArea;
        Debug.Log($"[SessionStateController] Area Changed -> {currentArea}");

        OnAreaChanged?.Invoke(currentArea);
    }

    public void EnterBase()
    {
        SetArea(SessionAreaType.Base);
    }

    public void EnterRaid()
    {
        SetArea(SessionAreaType.Raid);
    }
}