using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Camera Offset")]
    public Vector3 offset = new Vector3(0f, 18f, -12f);

    [Header("Smooth Follow")]
    public float smoothSpeed = 8f;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.rotation = Quaternion.Euler(55f, 0f, 0f);
    }
}