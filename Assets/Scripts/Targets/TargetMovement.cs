using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    [SerializeField] private Vector3 axis = Vector3.right;
    [SerializeField] private float distance = 2f, speed = 1f;
    private Vector3 origin; private void Start() { origin = transform.position; }
    public void Configure(Vector3 moveAxis, float moveDistance, float moveSpeed) { axis = moveAxis; distance = moveDistance; speed = moveSpeed; origin = transform.position; }
    private void Update() { transform.position = origin + axis.normalized * Mathf.Sin(Time.time * speed) * distance; }
}
