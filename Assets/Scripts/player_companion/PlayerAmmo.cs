using UnityEngine;
using TMPro;

public class PlayerAmmo : MonoBehaviour
{
    [Header("Ammo")]
    public int magazineSize = 30;
    public int currentAmmo;
    public float reloadTime = 1.2f;

    private bool isReloading = false;
    private float reloadTimer = 0f;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    [Header("Ammo Bar UI")]
    public RectTransform ammoBarFill;
    public GameObject ammoBarBackground;
    public float reloadBlinkInterval = 0.15f;

    private float blinkTimer = 0f;
    private bool blinkState = true;

    private Vector3 originalBarScale;

    void Start()
    {
        currentAmmo = magazineSize;

        if (ammoBarFill != null)
        {
            originalBarScale = ammoBarFill.localScale;
        }

        UpdateAmmoUI();
        UpdateAmmoBar();
    }

    void Update()
    {
        UpdateReload();
        HandleReloadInput();
        HandleReloadBlink();
    }

    void HandleReloadInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isReloading) return;
            if (currentAmmo == magazineSize) return;

            StartReload();
        }
    }

    void StartReload()
    {
        isReloading = true;
        reloadTimer = reloadTime;

        blinkTimer = reloadBlinkInterval;
        blinkState = true;

        if (ammoBarBackground != null)
        {
            ammoBarBackground.SetActive(true);
        }
    }

    void UpdateReload()
    {
        if (!isReloading) return;

        reloadTimer -= Time.deltaTime;

        if (reloadTimer <= 0f)
        {
            isReloading = false;
            currentAmmo = magazineSize;

            if (ammoBarBackground != null)
            {
                ammoBarBackground.SetActive(true);
            }

            UpdateAmmoUI();
            UpdateAmmoBar();
        }
    }

    void HandleReloadBlink()
    {
        if (!isReloading) return;
        if (ammoBarBackground == null) return;

        blinkTimer -= Time.deltaTime;

        if (blinkTimer <= 0f)
        {
            blinkTimer = reloadBlinkInterval;
            blinkState = !blinkState;
            ammoBarBackground.SetActive(blinkState);
        }
    }

    public bool TryUseAmmo(int amount)
    {
        if (isReloading) return false;
        if (currentAmmo < amount) return false;

        currentAmmo -= amount;

        UpdateAmmoUI();
        UpdateAmmoBar();

        return true;
    }

    public bool CanShoot()
    {
        return !isReloading && currentAmmo > 0;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + magazineSize;
        }
    }

    void UpdateAmmoBar()
    {
        if (ammoBarFill != null)
        {
            float ammoPercent = (float)currentAmmo / magazineSize;
            ammoBarFill.localScale = new Vector3(
                originalBarScale.x,
                originalBarScale.y * ammoPercent,
                originalBarScale.z
            );
        }
    }

    public void ResetRuntimeStateAfterRespawn()
    {
        isReloading = false;
        reloadTimer = 0f;

        currentAmmo = magazineSize;

        if (ammoBarBackground != null)
            ammoBarBackground.SetActive(true);

        UpdateAmmoUI();
        UpdateAmmoBar();
    }
}