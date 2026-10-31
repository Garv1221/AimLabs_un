using System.Collections;
using System;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField, Min(1)] private int magazineSize = 10;
    [SerializeField, Min(0)] private int reserveStart = 50;
    [SerializeField, Min(0.05f)] private float cooldown = 0.2f;
    [SerializeField, Min(0.1f)] private float reloadDuration = 2f;
    [SerializeField, Min(1f)] private float range = 100f;
    [SerializeField] private LayerMask shootingLayers = Physics.DefaultRaycastLayers;
    [SerializeField] private bool drawDebugRay;
    [SerializeField, Min(0f)] private float debugRayDuration = 0.5f;
    [SerializeField] private GameObject muzzleFlash;

    private int magazine;
    private int reserve;
    private float nextShot;
    private bool reloading;

    public int Magazine => magazine;
    public int Reserve => reserve;
    public bool Reloading => reloading;

    private void Awake()
    {
        if (!aimCamera) aimCamera = GetComponentInParent<Camera>();
        if (!aimCamera) aimCamera = FindAnyObjectByType<Camera>();
        // Older serialized scene objects may predate this field and deserialize it as zero.
        if (shootingLayers.value == 0) shootingLayers = Physics.DefaultRaycastLayers;
    }

    private void Start() => ResetAmmo();

    public void Configure(Camera cam, GameObject flash = null)
    {
        aimCamera = cam;
        if (flash) muzzleFlash = flash;
    }

    public void ResetAmmo()
    {
        StopAllCoroutines();
        magazine = magazineSize;
        reserve = reserveStart;
        reloading = false;
        nextShot = 0f;
        RefreshHud();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) TryReload();
        if (Cursor.lockState == CursorLockMode.Locked && Input.GetMouseButton(0)) TryFire();
    }

    /// <summary>Fires one bullet along the center ray of the assigned camera.</summary>
    public bool TryFire()
    {
        if (!aimCamera || reloading || magazine <= 0 || Time.time < nextShot) return false;

        magazine--;
        nextShot = Time.time + cooldown;
        ShowMuzzleFlash();

        bool hitTarget = false;
        int awardedPoints = 0;
        Ray ray = new Ray(aimCamera.transform.position, aimCamera.transform.forward);
        if (drawDebugRay) Debug.DrawRay(ray.origin, ray.direction * range, Color.red, debugRayDuration);
        RaycastHit[] hits = Physics.RaycastAll(ray, range, shootingLayers, QueryTriggerInteraction.Ignore);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        Transform playerRoot = aimCamera.transform.root;
        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.collider.transform;
            if (hitTransform == playerRoot || hitTransform.IsChildOf(playerRoot)) continue;

            Target target = hit.collider.GetComponentInParent<Target>();
            if (target && !target.isActiveAndEnabled) continue;
            if (target && target.TryHit(out awardedPoints)) hitTarget = true;
            // The closest non-player collider blocks this shot even when it is not a target.
            break;
        }

        if (ScoreManager.Instance) ScoreManager.Instance.RecordShot(hitTarget, awardedPoints);
        if (hitTarget && UIManager.Instance) UIManager.Instance.ShowHitFeedback(awardedPoints);
        RefreshHud();
        return hitTarget;
    }

    public void TryReload()
    {
        if (!reloading && magazine < magazineSize && reserve > 0) StartCoroutine(Reload());
    }

    private IEnumerator Reload()
    {
        reloading = true;
        RefreshHud();
        yield return new WaitForSeconds(reloadDuration);

        int transfer = Mathf.Min(magazineSize - magazine, reserve);
        magazine += transfer;
        reserve -= transfer;
        reloading = false;
        RefreshHud();
    }

    private void ShowMuzzleFlash()
    {
        if (!muzzleFlash) return;
        muzzleFlash.SetActive(true);
        CancelInvoke(nameof(HideMuzzleFlash));
        Invoke(nameof(HideMuzzleFlash), 0.06f);
    }

    private void HideMuzzleFlash()
    {
        if (muzzleFlash) muzzleFlash.SetActive(false);
    }

    private void RefreshHud()
    {
        if (UIManager.Instance) UIManager.Instance.Refresh();
    }
}
