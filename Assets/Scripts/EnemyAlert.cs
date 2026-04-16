using UnityEngine;

public class EnemyAlert : MonoBehaviour
{
    public GameObject alertCanvas;
    public float alertDuration = 1f;

    private float alertTimer = 0f;

    void Update()
    {
        if (alertCanvas == null) return;

        if (alertCanvas.activeSelf)
        {
            alertTimer -= Time.deltaTime;

            if (alertTimer <= 0f)
            {
                alertCanvas.SetActive(false);
            }
        }
    }

    public void ShowAlert()
    {
        if (alertCanvas == null) return;

        alertCanvas.SetActive(true);
        alertTimer = alertDuration;
    }
}