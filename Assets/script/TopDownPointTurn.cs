using UnityEngine;

public class TopDownPointTurn : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform model;

    [Header("Ground")]
    public LayerMask groundLayer;

    [Header("Turn")]
    public float turnSpeed = 15f;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        TurnToMousePoint();
    }

    private void TurnToMousePoint()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 200f, groundLayer))
        {
            Vector3 direction = hit.point - model.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            model.rotation = Quaternion.Slerp(
                model.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }
}