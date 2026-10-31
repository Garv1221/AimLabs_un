using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [SerializeField] private Transform playerBody;
    [SerializeField, Min(0.01f)] private float sensitivity = 2f;
    [SerializeField, Range(1f, 89f)] private float pitchLimit = 85f;

    private float pitch;

    private void Awake()
    {
        if (!playerBody && transform.parent) playerBody = transform.parent;
        pitch = NormalizeAngle(transform.localEulerAngles.x);
    }

    private void Update()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        float yawDelta = Input.GetAxisRaw("Mouse X") * sensitivity;
        float pitchDelta = Input.GetAxisRaw("Mouse Y") * sensitivity;
        pitch = Mathf.Clamp(pitch - pitchDelta, -pitchLimit, pitchLimit);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        if (playerBody) playerBody.Rotate(Vector3.up * yawDelta);
    }

    public void Configure(Transform body)
    {
        playerBody = body;
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}
