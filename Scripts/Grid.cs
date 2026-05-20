using System;
using System.Collections.Generic;

public class Grid
{
    public struct MoveInfo
    {
        public int FromRow, FromCol;
        public int ToRow, ToCol;
        public bool IsMerge;
        public int Value;
    }

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

    public static List<MoveInfo> ComputeMoves(int[,] oldState, int[,] newState, Direction dir) {
        var moves = new List<MoveInfo>();

        for (int i = 0; i < Size; i++) {
            var oldLine = GetLineValues(oldState, i, dir);
            var newLine = GetLineValues(newState, i, dir);

            // Collect non-zero entries with their positions
            var oldNonZero = new List<(int pos, int val)>();
            var newNonZero = new List<(int pos, int val)>();
            for (int p = 0; p < Size; p++) {
                if (oldLine[p] != 0) oldNonZero.Add((p, oldLine[p]));
                if (newLine[p] != 0) newNonZero.Add((p, newLine[p]));
            }

            int oi = 0;
            foreach (var (newPos, newVal) in newNonZero) {
                int fromRow1, fromCol1, fromRow2, fromCol2, toRow, toCol;
                LinePosToCell(i, newPos, dir, out toRow, out toCol);

                if (oi + 1 < oldNonZero.Count
                    && oldNonZero[oi].val == oldNonZero[oi + 1].val
                    && oldNonZero[oi].val * 2 == newVal) {
                    // Merge: two old tiles → one new tile
                    LinePosToCell(i, oldNonZero[oi].pos, dir, out fromRow1, out fromCol1);
                    LinePosToCell(i, oldNonZero[oi + 1].pos, dir, out fromRow2, out fromCol2);

                    moves.Add(new MoveInfo {
                        FromRow = fromRow1, FromCol = fromCol1, ToRow = toRow, ToCol = toCol, IsMerge = true,
                        Value = oldNonZero[oi].val
                    });
                    moves.Add(new MoveInfo {
                        FromRow = fromRow2, FromCol = fromCol2, ToRow = toRow, ToCol = toCol, IsMerge = true,
                        Value = oldNonZero[oi + 1].val
                    });
                    oi += 2;
                } else if (oi < oldNonZero.Count && oldNonZero[oi].val == newVal) {
                    // Simple move
                    LinePosToCell(i, oldNonZero[oi].pos, dir, out fromRow1, out fromCol1);
                    moves.Add(new MoveInfo {
                        FromRow = fromRow1, FromCol = fromCol1, ToRow = toRow, ToCol = toCol, IsMerge = false,
                        Value = newVal
                    });
                    oi++;
                }
            }
        }

        return moves;
    }

    private static int[] GetLineValues(int[,] state, int index, Direction dir) {
        int[] line = new int[Size];
        for (int i = 0; i < Size; i++) {
            line[i] = dir switch {
                Direction.Left => state[index, i],
                Direction.Right => state[index, Size - 1 - i],
                Direction.Up => state[i, index],
                Direction.Down => state[Size - 1 - i, index],
                _ => 0
            };
        }

        return line;
    }

    private static void LinePosToCell(int lineIndex, int pos, Direction dir, out int row, out int col) {
        switch (dir) {
            case Direction.Left:
                row = lineIndex;
                col = pos;
                break;
            case Direction.Right:
                row = lineIndex;
                col = Size - 1 - pos;
                break;
            case Direction.Up:
                row = pos;
                col = lineIndex;
                break;
            case Direction.Down:
                row = Size - 1 - pos;
                col = lineIndex;
                break;
            default: row = col = 0; break;
        }
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