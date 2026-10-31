using UnityEngine;

/// <summary>Initializes the single-scene training range and controls cursor capture.</summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private UIManager ui;
    [SerializeField] private TargetSpawner spawner;
    private bool cursorReleasedByEscape;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;
        if (!ui) ui = FindAnyObjectByType<UIManager>();
        if (!spawner) spawner = FindAnyObjectByType<TargetSpawner>();
    }

    private void Start()
    {
        if (ScoreManager.Instance) ScoreManager.Instance.ResetRound();
        GunController gun = FindAnyObjectByType<GunController>();
        if (gun) gun.ResetAmmo();
        if (spawner) spawner.Begin();
        LockCursor();
        if (ui) ui.Refresh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked) UnlockCursor();
            else LockCursor();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !cursorReleasedByEscape) LockCursor();
    }

    public void LockCursor()
    {
        cursorReleasedByEscape = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        cursorReleasedByEscape = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
