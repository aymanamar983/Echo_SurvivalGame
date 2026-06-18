using UnityEngine;

public class TopDownMouseAim : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform model;

    [Header("Aim Settings")]
    public LayerMask groundLayer;
    public float rotateSpeed = 20f;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        AimAtMouse();
    }

    private void AimAtMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 lookDir = hit.point - transform.position;
            lookDir.y = 0f;

            if (lookDir.sqrMagnitude < 0.01f) return;

            Quaternion targetRot = Quaternion.LookRotation(lookDir);

            model.rotation = Quaternion.Slerp(
                model.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
    }
}