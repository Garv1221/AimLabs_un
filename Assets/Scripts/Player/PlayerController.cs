using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (!controller) controller = gameObject.AddComponent<CharacterController>();
    }

    private void Update()
    {
        if (!controller || !controller.enabled) return;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 planarInput = transform.right * horizontal + transform.forward * vertical;
        Vector3 planarMovement = Vector3.ClampMagnitude(planarInput, 1f) * moveSpeed;

        if (controller.isGrounded && verticalVelocity.y < 0f) verticalVelocity.y = -2f;
        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move((planarMovement + verticalVelocity) * Time.deltaTime);
    }
}
