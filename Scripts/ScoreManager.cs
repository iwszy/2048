using Godot;

public class ScoreManager
{
    private const string SavePath = "user://best_score.cfg";
    private const string Section = "score";
    private const string Key = "best";

    public int CurrentScore { get; private set; }
    public int BestScore { get; private set; }

    public ScoreManager() {
        LoadBestScore();
    }

    public void AddScore(int points) {
        CurrentScore += points;
        if (CurrentScore > BestScore) {
            BestScore = CurrentScore;
            SaveBestScore();
        }
    }

    public void SetScore(int score) {
        CurrentScore = score;
    }

    public void ResetScore() {
        CurrentScore = 0;
    }

    private void LoadBestScore() {
        if (FileAccess.FileExists(SavePath)) {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            BestScore = (int)file.Get32();
        }
    }

    private void SaveBestScore() {
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        file.Store32((uint)BestScore);
    }
}