using System.Collections.Generic;

public class UndoManager
{
    private const int MaxDepth = 3;

    public struct Snapshot
    {
        public int[,] GridState;
        public int Score;
    }

    private readonly Queue<Snapshot> _queue = new();

    public int Count => _queue.Count;
    public bool CanUndo => _queue.Count > 0;

    public void Push(int[,] gridState, int score) {
        if (_queue.Count >= MaxDepth) {
            _queue.Dequeue();
        }

        _queue.Enqueue(new Snapshot {
            GridState = gridState,
            Score = score
        });
    }

    public Snapshot? Pop() {
        if (_queue.Count == 0) {
            return null;
        }

        // Dequeue all into a list, take last, rebuild queue without it
        var all = _queue.ToArray();
        _queue.Clear();

        // Re-enqueue all except the last one
        for (int i = 0; i < all.Length - 1; i++) {
            _queue.Enqueue(all[i]);
        }
        return all[^1];
    }

    public void Clear() {
        _queue.Clear();
    }
}