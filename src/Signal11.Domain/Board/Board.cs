namespace Signal11.Domain;

/// <summary>
/// Immutable in-memory representation of an SN11 board.
/// Cells are indexed board[row, col] where row 0 is the top.
/// </summary>
public sealed class Board
{
    public int Width  { get; }
    public int Height { get; }
    public IReadOnlyList<(int X, int Y)> Flags { get; }

    private readonly Cell[,] _cells;

    public Cell this[int row, int col] => _cells[row, col];

    public Board(int width, int height, Cell[,] cells, (int X, int Y)[] flags)
    {
        if (width  <= 0) throw new ArgumentOutOfRangeException(nameof(width),  "Board width must be at least 1.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Board height must be at least 1.");
        if (cells.GetLength(0) != height || cells.GetLength(1) != width)
            throw new ArgumentException(
                $"cells dimensions [{cells.GetLength(0)},{cells.GetLength(1)}] do not match {width}×{height}.",
                nameof(cells));

        Width  = width;
        Height = height;
        Flags  = Array.AsReadOnly(flags);
        _cells = (Cell[,])cells.Clone();
    }
}
