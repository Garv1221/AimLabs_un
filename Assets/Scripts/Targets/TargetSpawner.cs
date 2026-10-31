using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public static TargetSpawner Instance { get; private set; }

    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Transform parent;
    [SerializeField, Min(1)] private int maximumActiveTargets = 5;
    [SerializeField, Min(0f)] private float respawnDelay = 0.75f;
    [SerializeField, Min(0)] private int pointsPerHit = 10;
    [SerializeField, Min(1)] private int spawnPositionAttempts = 24;

    private readonly HashSet<Target> activeTargets = new HashSet<Target>();
    private bool isSpawning;

    public int ActiveTargetCount
    {
        get
        {
            RemoveDestroyedTargets();
            return activeTargets.Count;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Configure(GameObject prefab, Transform container)
    {
        targetPrefab = prefab;
        parent = container;
    }

    public void Begin()
    {
        if (isSpawning) return;
        if (!targetPrefab)
        {
            Debug.LogError("TargetSpawner has no target prefab assigned.", this);
            return;
        }

        if (!parent) parent = transform;
        isSpawning = true;
        activeTargets.Clear();

        Target[] existingTargets = parent.GetComponentsInChildren<Target>(false);
        foreach (Target target in existingTargets)
        {
            if (!target || !target.isActiveAndEnabled || !NormalizeTargetColliders(target)) continue;
            target.Configure(pointsPerHit);
            RegisterTarget(target);
            TargetMovement movement = target.GetComponentInChildren<TargetMovement>(true);
            if (movement) movement.enabled = false;
        }

        while (activeTargets.Count < maximumActiveTargets)
        {
            if (!SpawnTarget()) break;
        }
    }

    public void RegisterTarget(Target target)
    {
        if (target && target.isActiveAndEnabled && parent && target.transform.IsChildOf(parent))
            activeTargets.Add(target);
    }

    public void NotifyTargetHit(Target target)
    {
        RemoveTargetAndScheduleRespawn(target);
    }

    public void NotifyTargetDisabled(Target target)
    {
        RemoveTargetAndScheduleRespawn(target);
    }

    private void RemoveTargetAndScheduleRespawn(Target target)
    {
        if (!target || !activeTargets.Remove(target) || !isSpawning) return;
        StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        if (respawnDelay > 0f) yield return new WaitForSeconds(respawnDelay);
        RemoveDestroyedTargets();
        if (isSpawning && activeTargets.Count < maximumActiveTargets) SpawnTarget();
    }

    private bool SpawnTarget()
    {
        if (!isSpawning || !targetPrefab || !parent || activeTargets.Count >= maximumActiveTargets)
            return false;

        if (!TryGetSpawnPosition(out Vector3 position))
        {
            Debug.LogWarning("No clear position was found for a replacement target.", this);
            return false;
        }

        GameObject instance = Instantiate(targetPrefab, position, Quaternion.identity, parent);
        instance.name = "Practice Target";
        instance.SetActive(true);

        Target target = instance.GetComponentInChildren<Target>(true);
        Collider[] colliders = instance.GetComponentsInChildren<Collider>(true);
        if (!target || !target.isActiveAndEnabled || !NormalizeTargetColliders(target))
        {
            Debug.LogError("Spawned target must have an active Target component and enabled Collider.", instance);
            Destroy(instance);
            return false;
        }

        target.Configure(pointsPerHit);
        RegisterTarget(target);
        TargetMovement movement = instance.GetComponentInChildren<TargetMovement>(true);
        if (movement) movement.enabled = false;
        return true;
    }

    private static bool NormalizeTargetColliders(Target target)
    {
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        bool hasEnabledCollider = false;
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        foreach (Collider targetCollider in colliders)
        {
            if (!targetCollider) continue;
            if (targetCollider.gameObject.activeInHierarchy)
            {
                targetCollider.enabled = true;
                targetCollider.isTrigger = false;
                hasEnabledCollider = true;
            }
            if (targetCollider.gameObject.layer == ignoreRaycastLayer)
                targetCollider.gameObject.layer = 0;
        }

        return hasEnabledCollider;
    }

    private bool TryGetSpawnPosition(out Vector3 position)
    {
        for (int attempt = 0; attempt < spawnPositionAttempts; attempt++)
        {
            position = new Vector3(Random.Range(-9f, 9f), Random.Range(1.4f, 3.8f), Random.Range(14f, 25f));
            bool overlapsTarget = false;
            foreach (Target target in activeTargets)
            {
                if (target && (target.transform.position - position).sqrMagnitude < 1.75f * 1.75f)
                {
                    overlapsTarget = true;
                    break;
                }
            }

            if (!overlapsTarget) return true;
        }

        position = Vector3.zero;
        return false;
    }

    private void RemoveDestroyedTargets()
    {
        activeTargets.RemoveWhere(target => !target || !target.isActiveAndEnabled);
    }

    private void OnDestroy()
    {
        isSpawning = false;
        activeTargets.Clear();
        if (Instance == this) Instance = null;
    }
}
