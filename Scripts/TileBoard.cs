using Godot;

public partial class TileBoard : Control
{
    private const float Spacing = 12f;
    private const int GridSize = Grid.Size;
    private const float BoardSize = GridSize * TileDisplay.TileSize + (GridSize + 1) * Spacing;

    private readonly TileDisplay[,] _tiles = new TileDisplay[GridSize, GridSize];

    public override void _Ready() {
        CustomMinimumSize = new Vector2(BoardSize, BoardSize);

        // Board background
        var bg = new ColorRect();
        bg.Color = new Color("BBADA0");
        bg.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        // Create 16 tiles
        for (int r = 0; r < GridSize; r++) {
            for (int c = 0; c < GridSize; c++) {
                var tile = new TileDisplay();
                tile.Position = new Vector2(
                    Spacing + c * (TileDisplay.TileSize + Spacing),
                    Spacing + r * (TileDisplay.TileSize + Spacing)
                );
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
}