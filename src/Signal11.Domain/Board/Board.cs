namespace Signal11.Domain;

/// <summary>
/// Immutable in-memory representation of an SN11 board.
/// Cells are stored row-major: Cells[row, col] where row 0 is the top.
/// </summary>
public record Board(
    int Width,
    int Height,
    Cell[,] Cells,
    (int X, int Y)[] Flags
);
