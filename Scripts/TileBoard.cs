using System;
using System.Collections.Generic;
using Godot;

public partial class TileBoard : Control
{
    private const int GridSize = Grid.Size;
    private const float TileSize = TileDisplay.TileSize;
    private const float Spacing = 10f;
    private const float BoardSize = GridSize * TileSize + (GridSize + 1) * Spacing;

    private readonly TileDisplay[,] _tiles = new TileDisplay[GridSize, GridSize];
    private readonly List<Control> _animTiles = new();
    private Tween _activeTween;

    public override void _Ready() {
        CustomMinimumSize = new Vector2(BoardSize, BoardSize);

        for (int r = 0; r < GridSize; r++) {
            for (int c = 0; c < GridSize; c++) {
                var tile = new TileDisplay();
                tile.Position = CellPosition(r, c);
                AddChild(tile);
                _tiles[r, c] = tile;
            }
        }
    }

    public void RefreshBoard(int[,] gridState) {
        for (int r = 0; r < GridSize; r++) {
            for (int c = 0; c < GridSize; c++) {
                _tiles[r, c].SetValue(gridState[r, c]);
            }
        }
    }

    public void AnimateSpawn(int row, int col) {
        _tiles[row, col].PlaySpawnAnimation();
    }

    public void AnimateMove(List<Grid.MoveInfo> moves, int[,] newState, Action onComplete) {
        _activeTween?.Kill();
        ClearAnimTiles();

        // Set all real tiles to empty — they stay at home positions as background
        for (int r = 0; r < GridSize; r++)
        for (int c = 0; c < GridSize; c++)
            _tiles[r, c].SetValue(0);

        // Collect merge destinations
        var mergeDestinations = new HashSet<(int r, int c)>();
        foreach (var move in moves) {
            if (move.IsMerge) {
                mergeDestinations.Add((move.ToRow, move.ToCol));
            }
        }

        // Create animated tiles that slide from source to destination
        foreach (var move in moves) {
            var anim = MakeAnimTile(move.Value);
            anim.Position = CellPosition(move.FromRow, move.FromCol);
            AddChild(anim);
            _animTiles.Add(anim);
        }

        var tween = CreateTween();
        _activeTween = tween;

        // Phase 1: slide all anim tiles in parallel
        tween.SetParallel(true);
        for (int i = 0; i < moves.Count; i++) {
            var targetPos = CellPosition(moves[i].ToRow, moves[i].ToCol);
            tween.TweenProperty(_animTiles[i], "position", targetPos, 0.02f)
                .SetEase(Tween.EaseType.InOut);
        }

        // Phase 2: set destination values, pop merged tiles
        tween.Chain().TweenCallback(Callable.From(() => {
            ClearAnimTiles();

            foreach (var move in moves) {
                int r = move.ToRow, c = move.ToCol;
                _tiles[r, c].SetValue(newState[r, c]);
                if (move.IsMerge)
                    _tiles[r, c].Scale = new Vector2(1.3f, 1.3f);
            }
        }));
        tween.SetParallel(true);
        foreach (var (r, c) in mergeDestinations) {
            tween.TweenProperty(_tiles[r, c], "scale", Vector2.One, 0.2f)
                .From(new Vector2(1.3f, 1.3f))
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
        }

        // Phase 3: finalize
        tween.Chain().TweenCallback(Callable.From(() => {
            _activeTween = null;
            ClearAnimTiles();
            RefreshBoard(newState);
            for (int r = 0; r < GridSize; r++)
            for (int c = 0; c < GridSize; c++)
                _tiles[r, c].Scale = Vector2.One;
            onComplete();
        }));
    }

    private Control MakeAnimTile(int value) {
        var ctrl = new Control();
        ctrl.SetSize(new Vector2(TileSize, TileSize));
        ctrl.MouseFilter = MouseFilterEnum.Ignore;

        var bg = new ColorRect();
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        bg.Color = GetTileColor(value);
        ctrl.AddChild(bg);

        var label = new Label();
        label.Text = value.ToString();
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.SetAnchorsPreset(LayoutPreset.FullRect);
        label.Modulate = value <= 4 ? new Color("3C3A32") : Colors.White;

        if (value >= 1000) label.AddThemeFontSizeOverride("font_size", 22);
        else if (value >= 100) label.AddThemeFontSizeOverride("font_size", 28);
        else label.AddThemeFontSizeOverride("font_size", 32);

        ctrl.AddChild(label);
        return ctrl;
    }

    private void ClearAnimTiles() {
        foreach (var t in _animTiles) {
            t.QueueFree();
        }

        _animTiles.Clear();
    }

    private static Vector2 CellPosition(int row, int col) {
        return new Vector2(
            Spacing + col * (TileSize + Spacing),
            Spacing + row * (TileSize + Spacing)
        );
    }

    private static Color GetTileColor(int value) => value switch {
        0 => new Color("CDC1B4"),
        2 => new Color("EEE4DA"),
        4 => new Color("EDE0C8"),
        8 => new Color("F2B179"),
        16 => new Color("F59563"),
        32 => new Color("F67C5F"),
        64 => new Color("F65E3B"),
        128 => new Color("EDCF72"),
        256 => new Color("EDCC61"),
        512 => new Color("EDC850"),
        1024 => new Color("EDC53F"),
        2048 => new Color("EDC22E"),
        _ => new Color("3C3A32"),
    };
}