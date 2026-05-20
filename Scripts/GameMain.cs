using Godot;

public partial class GameMain : Control
{
    private Grid _grid;
    private UndoManager _undo;
    private ScoreManager _scores;

    private TileBoard _tileBoard;
    private Label _scoreLabel;
    private Label _bestLabel;
    private Button _undoButton;
    private Control _gameOverOverlay;
    private Label _gameOverScoreLabel;
    private bool _isGameOver;

    public override void _Ready() {
        _grid = new Grid();
        _undo = new UndoManager();
        _scores = new ScoreManager();

        _tileBoard = GetNode<TileBoard>("VBoxContainer/TileBoard");
        _scoreLabel = GetNode<Label>("VBoxContainer/HeaderRow/ScorePanel/ScoreValue");
        _bestLabel = GetNode<Label>("VBoxContainer/HeaderRow/BestPanel/BestValue");
        _undoButton = GetNode<Button>("VBoxContainer/ButtonRow/UndoButton");
        _gameOverOverlay = GetNode<Control>("GameOverOverlay");
        _gameOverScoreLabel = GetNode<Label>("GameOverOverlay/VBoxOverlay/ScoreLabel");

        GetNode<Button>("VBoxContainer/ButtonRow/NewGameButton").Pressed += OnNewGamePressed;
        _undoButton.Pressed += OnUndoPressed;
        GetNode<Button>("GameOverOverlay/VBoxOverlay/TryAgainButton").Pressed += OnRestartPressed;

        NewGame();
    }

    public override void _Input(InputEvent @event) {
        if (_isGameOver) return;

        if (Input.IsActionJustPressed("move_left"))
            PerformMove(Grid.Direction.Left);
        else if (Input.IsActionJustPressed("move_right"))
            PerformMove(Grid.Direction.Right);
        else if (Input.IsActionJustPressed("move_up"))
            PerformMove(Grid.Direction.Up);
        else if (Input.IsActionJustPressed("move_down"))
            PerformMove(Grid.Direction.Down);
        else if (Input.IsActionJustPressed("ui_undo"))
            UndoMove();
    }

    private void NewGame() {
        _grid.Initialize();
        _scores.ResetScore();
        _undo.Clear();
        _isGameOver = false;
        _gameOverOverlay.Visible = false;

        var cells = _grid.Cells;
        _tileBoard.RefreshBoard(cells);

        for (int r = 0; r < Grid.Size; r++) {
            for (int c = 0; c < Grid.Size; c++) {
                if (cells[r, c] != 0) {
                    _tileBoard.AnimateSpawn(r, c);
                }
            }
        }

        UpdateScoreDisplay();
        UpdateUndoButton();
    }

    private void PerformMove(Grid.Direction dir) {
        _undo.Push(_grid.GetStateCopy(), _scores.CurrentScore);

        var (changed, scoreGained) = _grid.TryMove(dir);

        if (!changed) {
            _undo.Pop();
            return;
        }

        _scores.AddScore(scoreGained);

        var spawn = _grid.SpawnTile();
        _tileBoard.RefreshBoard(_grid.Cells);

        if (spawn.HasValue)
            _tileBoard.AnimateSpawn(spawn.Value.row, spawn.Value.col);

        if (_grid.IsGameOver())
            OnGameOver();

        UpdateScoreDisplay();
        UpdateUndoButton();
    }

    private void UndoMove() {
        var snapshot = _undo.Pop();
        if (snapshot == null) return;

        _grid.RestoreState(snapshot.Value.GridState);
        _scores.SetScore(snapshot.Value.Score);
        _tileBoard.RefreshBoard(_grid.Cells);

        if (_isGameOver) {
            _isGameOver = false;
            _gameOverOverlay.Visible = false;
        }

        UpdateScoreDisplay();
        UpdateUndoButton();
    }

    private void OnGameOver() {
        _isGameOver = true;
        _gameOverScoreLabel.Text = $"Score: {_scores.CurrentScore}";
        _gameOverOverlay.Visible = true;
    }

    private void UpdateScoreDisplay() {
        _scoreLabel.Text = _scores.CurrentScore.ToString();
        _bestLabel.Text = _scores.BestScore.ToString();
    }

    private void UpdateUndoButton() {
        _undoButton.Disabled = !_undo.CanUndo;
    }

    private void OnNewGamePressed() {
        NewGame();
    }

    private void OnUndoPressed() {
        UndoMove();
    }

    private void OnRestartPressed() {
        NewGame();
    }
}