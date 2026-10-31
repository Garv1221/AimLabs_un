using UnityEngine;

[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class Target : MonoBehaviour
{
    [SerializeField, Min(0)] private int points = 10;
    private bool consumed;

    public int Points => points;

    private void OnEnable()
    {
        if (TargetSpawner.Instance) TargetSpawner.Instance.RegisterTarget(this);
    }

    private void OnDisable()
    {
        if (!consumed && TargetSpawner.Instance) TargetSpawner.Instance.NotifyTargetDisabled(this);
    }

    public void Configure(int value)
    {
        points = Mathf.Max(0, value);
        consumed = false;
    }

    /// <summary>Returns true once, then removes this target from the range.</summary>
    public bool TryHit(out int awardedPoints)
    {
        awardedPoints = 0;
        if (consumed || !isActiveAndEnabled) return false;

        consumed = true;
        awardedPoints = points;
        if (TargetSpawner.Instance) TargetSpawner.Instance.NotifyTargetHit(this);
        gameObject.SetActive(false);
        Destroy(gameObject);
        return true;
    }

    private void OnValidate()
    {
        points = Mathf.Max(0, points);
        if (gameObject.layer == LayerMask.NameToLayer("Ignore Raycast"))
            Debug.LogWarning($"Target '{name}' is on the Ignore Raycast layer and cannot be shot.", this);
    }
}
