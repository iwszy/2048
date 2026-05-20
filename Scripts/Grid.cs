using System;
using System.Collections.Generic;

public class Grid
{
    public const int Size = 4;

    public enum Direction
    {
        Left,
        Right,
        Up,
        Down
    }

    private readonly int[,] _cells = new int[Size, Size];

    public int[,] Cells {
        get {
            var copy = new int[Size, Size];
            Array.Copy(_cells, copy, _cells.Length);
            return copy;
        }
    }

    public void Initialize() {
        Array.Clear(_cells, 0, _cells.Length);
        SpawnTile();
        SpawnTile();
    }

    public int[,] GetStateCopy() {
        return Cells;
    }

    public void RestoreState(int[,] state) {
        Array.Copy(state, _cells, _cells.Length);
    }

    public (bool changed, int scoreGained) TryMove(Direction dir) {
        bool changed = false;
        int scoreGained = 0;

        for (int i = 0; i < Size; i++) {
            int[] row = GetLine(i, dir);
            var (result, lineChanged, score) = ProcessLine(row);
            SetLine(i, dir, result);
            if (lineChanged) changed = true;
            scoreGained += score;
        }

        return (changed, scoreGained);
    }

    public (int row, int col, int value)? SpawnTile() {
        var empty = new List<(int r, int c)>();
        for (int r = 0; r < Size; r++)
        for (int c = 0; c < Size; c++)
            if (_cells[r, c] == 0)
                empty.Add((r, c));

        if (empty.Count == 0)
            return null;

        var (row, col) = empty[Random.Shared.Next(empty.Count)];
        int value = Random.Shared.NextDouble() < 0.9 ? 2 : 4;
        _cells[row, col] = value;
        return (row, col, value);
    }

    public bool IsGameOver() {
        for (int r = 0; r < Size; r++) {
            for (int c = 0; c < Size; c++) {
                if (_cells[r, c] == 0) {
                    return false;
                }
            }
        }

        for (int r = 0; r < Size; r++) {
            for (int c = 0; c < Size - 1; c++) {
                if (_cells[r, c] == _cells[r, c + 1]) {
                    return false;
                }
            }
        }

        for (int r = 0; r < Size - 1; r++) {
            for (int c = 0; c < Size; c++) {
                if (_cells[r, c] == _cells[r + 1, c]) {
                    return false;
                }
            }
        }

        return true;
    }

    private int[] GetLine(int index, Direction dir) {
        int[] line = new int[Size];
        for (int i = 0; i < Size; i++) {
            line[i] = dir switch {
                Direction.Left => _cells[index, i],
                Direction.Right => _cells[index, Size - 1 - i],
                Direction.Up => _cells[i, index],
                Direction.Down => _cells[Size - 1 - i, index],
                _ => 0
            };
        }

        return line;
    }

    private void SetLine(int index, Direction dir, int[] line) {
        for (int i = 0; i < Size; i++) {
            int value = line[i];
            int r = index, c = i;

            switch (dir) {
                case Direction.Left:
                    r = index;
                    c = i;
                    break;
                case Direction.Right:
                    r = index;
                    c = Size - 1 - i;
                    break;
                case Direction.Up:
                    r = i;
                    c = index;
                    break;
                case Direction.Down:
                    r = Size - 1 - i;
                    c = index;
                    break;
            }

            _cells[r, c] = value;
        }
    }

    private static (int[] result, bool changed, int score) ProcessLine(int[] line) {
        // 1. Strip zeros
        var compacted = new int[Size];
        int ci = 0;
        for (int i = 0; i < Size; i++)
            if (line[i] != 0)
                compacted[ci++] = line[i];

        int score = 0;

        // 2. Merge adjacent equal values
        for (int i = 0; i < Size - 1; i++) {
            if (compacted[i] != 0 && compacted[i] == compacted[i + 1]) {
                compacted[i] *= 2;
                score += compacted[i];
                compacted[i + 1] = 0;
                i++; // skip merged pair
            }
        }

        // 3. Strip zeros again after merge
        var result = new int[Size];
        ci = 0;
        for (int i = 0; i < Size; i++)
            if (compacted[i] != 0)
                result[ci++] = compacted[i];

        // 4. Check if changed
        bool changed = false;
        for (int i = 0; i < Size; i++)
            if (result[i] != line[i]) {
                changed = true;
                break;
            }

        return (result, changed, score);
    }
}