using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 15f, -10f);

    public float followSpeed = 10f;

    public float lookAheadDistance = 0.5f; // ← 추가

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 lookAhead = target.forward * lookAheadDistance;

        Vector3 desiredPosition = target.position + lookAhead + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
    }
}