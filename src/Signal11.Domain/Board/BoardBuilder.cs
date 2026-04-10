namespace Signal11.Domain;

/// <summary>
/// Fluent, in-memory board factory. Primarily used by test projects to
/// construct <see cref="Board"/> instances without touching disk.
/// </summary>
public sealed class BoardBuilder
{
    private readonly int _width;
    private readonly int _height;
    private readonly Cell[,] _cells;
    private readonly List<(int X, int Y)> _flags = new();

    private static readonly Cell DefaultCell =
        new(new NormalFloor(), Direction.None, false, 0, WallSide.None, WallSide.None);

    public BoardBuilder(int width, int height)
    {
        if (width  <= 0) throw new ArgumentOutOfRangeException(nameof(width),  "Board width must be at least 1.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Board height must be at least 1.");

        _width  = width;
        _height = height;
        _cells  = new Cell[height, width];

        for (int r = 0; r < height; r++)
        for (int c = 0; c < width;  c++)
            _cells[r, c] = DefaultCell;
    }

    public BoardBuilder SetFloor(int row, int col, CellFloor floor)
    {
        ValidateCoords(row, col);
        _cells[row, col] = _cells[row, col] with { Floor = floor };
        return this;
    }

    public BoardBuilder SetConveyor(int row, int col, Direction direction, bool isExpress = false)
    {
        ValidateCoords(row, col);
        _cells[row, col] = _cells[row, col] with { ConveyorDirection = direction, IsExpress = isExpress };
        return this;
    }

    public BoardBuilder SetGear(int row, int col, int gear)
    {
        ValidateCoords(row, col);
        if (gear is < 0 or > 2)
            throw new ArgumentOutOfRangeException(nameof(gear), gear, "Gear must be 0, 1, or 2.");
        _cells[row, col] = _cells[row, col] with { Gear = gear };
        return this;
    }

    public BoardBuilder SetWalls(int row, int col, WallSide walls)
    {
        ValidateCoords(row, col);
        _cells[row, col] = _cells[row, col] with { Walls = walls };
        return this;
    }

    public BoardBuilder SetLasers(int row, int col, WallSide lasers)
    {
        ValidateCoords(row, col);
        _cells[row, col] = _cells[row, col] with { WallLasers = lasers };
        return this;
    }

    private const int MaxFlags = 4;

    public BoardBuilder AddFlag(int x, int y)
    {
        if (_flags.Count >= MaxFlags)
            throw new ArgumentException($"A board may have at most {MaxFlags} flags.");
        _flags.Add((x, y));
        return this;
    }

    public Board Build() => new(_width, _height, _cells, _flags.ToArray());

    private void ValidateCoords(int row, int col)
    {
        if ((uint)row >= (uint)_height)
            throw new ArgumentOutOfRangeException(nameof(row), row, $"Row must be 0–{_height - 1}.");
        if ((uint)col >= (uint)_width)
            throw new ArgumentOutOfRangeException(nameof(col), col, $"Col must be 0–{_width - 1}.");
    }
}
