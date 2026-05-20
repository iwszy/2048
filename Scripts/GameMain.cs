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
    private Button _stopButton;
    private Control _gameOverOverlay;
    private Label _gameOverScoreLabel;
    private bool _isGameOver;

    // Mouse swipe tracking
    private Vector2 _swipeStart;
    private bool _isSwiping;
    private const float SwipeMinDistance = 50f;

    public override void _Ready() {
        _grid = new Grid();
        _undo = new UndoManager();
        _scores = new ScoreManager();

        _tileBoard = GetNode<TileBoard>("TileBoard");
        _scoreLabel = GetNode<Label>("Panel_Score/Label_Score");
        _bestLabel = GetNode<Label>("Panel_BestScore/Label_Score");
        _undoButton = GetNode<Button>("Button_Undo");
        _stopButton = GetNode<Button>("Button_StopGame");
        _gameOverOverlay = GetNode<Control>("GameOverOverlay");
        _gameOverScoreLabel = GetNode<Label>("GameOverOverlay/VBoxOverlay/ScoreLabel");

        GetNode<Button>("Button_NewGame").Pressed += OnNewGamePressed;
        _undoButton.Pressed += OnUndoPressed;
        _stopButton.Pressed += OnStopGamePressed;

        NewGame();
    }

    public override void _Input(InputEvent @event) {
        // Mouse swipe
        if (@event is InputEventMouseButton mb) {
            if (mb.ButtonIndex == MouseButton.Left) {
                if (mb.Pressed) {
                    _swipeStart = mb.Position;
                    _isSwiping = true;
                } else if (_isSwiping) {
                    _isSwiping = false;
                    var delta = mb.Position - _swipeStart;
                    if (delta.Length() >= SwipeMinDistance) {
                        HandleSwipe(delta);
                    }
                }
            }

            return;
        }

        if (_isGameOver) {
            return;
        }

        // Keyboard
        if (Input.IsActionJustPressed("move_left")) {
            PerformMove(Grid.Direction.Left);
        } else if (Input.IsActionJustPressed("move_right")) {
            PerformMove(Grid.Direction.Right);
        } else if (Input.IsActionJustPressed("move_up")) {
            PerformMove(Grid.Direction.Up);
        } else if (Input.IsActionJustPressed("move_down")) {
            PerformMove(Grid.Direction.Down);
        } else if (Input.IsActionJustPressed("ui_undo")) {
            UndoMove();
        }
    }

    private void HandleSwipe(Vector2 delta) {
        if (_isGameOver) {
            return;
        }

        if (Mathf.Abs(delta.X) > Mathf.Abs(delta.Y)) {
            PerformMove(delta.X > 0 ? Grid.Direction.Right : Grid.Direction.Left);
        } else {
            PerformMove(delta.Y > 0 ? Grid.Direction.Down : Grid.Direction.Up);
        }
    }

    private void NewGame() {
        _grid.Initialize();
        _scores.ResetScore();
        _undo.Clear();
        _isGameOver = false;
        _gameOverOverlay.Visible = false;
        _stopButton.Disabled = false;

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
            _stopButton.Disabled = false;
        }

        UpdateScoreDisplay();
        UpdateUndoButton();
    }

    private void OnGameOver() {
        _isGameOver = true;
        _gameOverScoreLabel.Text = $"分数：{_scores.CurrentScore}";
        _gameOverOverlay.Visible = true;
        _stopButton.Disabled = true;
    }

    private void StopGame() {
        if (_isGameOver) return;

        _isGameOver = true;
        _gameOverScoreLabel.Text = $"分数：{_scores.CurrentScore}";
        _gameOverOverlay.Visible = true;
        _stopButton.Disabled = true;
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

    private void OnStopGamePressed() {
        StopGame();
    }
}