using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int Score { get; private set; }
    public int Hits { get; private set; }
    public int Shots { get; private set; }
    public float Accuracy => Shots == 0 ? 0f : 100f * Hits / Shots;
    public int BestScore => PlayerPrefs.GetInt("TargetShooterBest", 0);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResetRound();
    }

    public void ResetRound()
    {
        Score = 0;
        Hits = 0;
        Shots = 0;
        RefreshHud();
    }

    public void RecordShot(bool hit, int points = 0)
    {
        Shots++;
        if (hit)
        {
            Hits++;
            Score += Mathf.Max(0, points);
            if (Score > BestScore)
            {
                PlayerPrefs.SetInt("TargetShooterBest", Score);
                PlayerPrefs.Save();
            }
        }

        RefreshHud();
    }

    private void RefreshHud()
    {
        if (UIManager.Instance) UIManager.Instance.Refresh();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
