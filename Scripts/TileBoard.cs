using Godot;

public partial class TileBoard : GridContainer
{
    private const int GridSize = Grid.Size;

    private readonly TileDisplay[,] _tiles = new TileDisplay[GridSize, GridSize];

    public override void _Ready() {
        Columns = GridSize;

        for (int r = 0; r < GridSize; r++) {
            for (int c = 0; c < GridSize; c++) {
                var tile = new TileDisplay();
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