using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Transform healthBarFill;

    private PlayerSquadBridge bridge;
    private bool hasHandledDeath = false;

    [Header("Hit Flash")]
    public float hitFlashDuration = 0.1f;
    private Renderer playerRenderer;
    private Color originalColor;
    private float hitFlashTimer = 0f;

    void Start()
    {
        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
        }
    }

    public void SetBridge(PlayerSquadBridge squadBridge)
    {
        bridge = squadBridge;
    }

    void Update()
    {
        HandleHitFlash();
        UpdateHealthBar();
        CheckDeathState();
    }

    public void TakeDamage(int damage)
    {
        if (bridge == null) return;

        bridge.ApplyIncomingDamage(damage);
        TriggerHitFlash();
    }

    void UpdateHealthBar()
    {
        if (bridge == null || healthBarFill == null) return;

        var squad = bridge.Squad;
        if (squad == null || squad.CombatState.MaxHealth <= 0) return;

        float percent = (float)squad.CombatState.CurrentHealth / squad.CombatState.MaxHealth;
        healthBarFill.localScale = new Vector3(percent, 1f, 1f);
    }

    void CheckDeathState()
    {
        if (bridge == null || bridge.Squad == null)
            return;

        if (bridge.Squad.CombatState.IsDead && !hasHandledDeath)
        {
            hasHandledDeath = true;
            HandleSquadDeath();
        }
    }

    void HandleSquadDeath()
    {
        Debug.Log("[PlayerHealth] 스쿼드 전멸 상태 반영");

        // 지금은 최소 처리만
        // 이후 여기서 입력 비활성화, 사망 연출, 게임오버 UI 호출 등을 연결
        gameObject.SetActive(false);
    }

    void TriggerHitFlash()
    {
        if (playerRenderer != null)
        {
            playerRenderer.material.color = Color.white;
            hitFlashTimer = hitFlashDuration;
        }
    }

    void HandleHitFlash()
    {
        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;

            if (hitFlashTimer <= 0f && playerRenderer != null)
            {
                playerRenderer.material.color = originalColor;
            }
        }
    }

    public void ResetAfterRespawn()
    {
        hasHandledDeath = false;

        // 히트 플래시 상태 초기화
        hitFlashTimer = 0f;

        if (playerRenderer != null)
        {
            playerRenderer.material.color = originalColor;
            playerRenderer.enabled = true;
        }

        // 플레이어 오브젝트가 꺼져 있었다면 다시 켜짐
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void ForceShowBody()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (playerRenderer != null)
        {
            playerRenderer.enabled = true;
            playerRenderer.material.color = originalColor;
        }
    }
}