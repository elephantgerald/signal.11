namespace Signal11.Domain;

/// <summary>
/// Discriminated union representing the floor type of a board cell.
/// The subtype carries all floor-specific data, making illegal states
/// (e.g. FlagNumber on a Normal cell) structurally unrepresentable.
/// </summary>
public abstract record CellFloor;

public record NormalFloor       : CellFloor;
public record PitFloor          : CellFloor;
public record RepairFloor       : CellFloor;
public record DoubleRepairFloor : CellFloor;

public record FlagFloor : CellFloor
{
    /// <summary>Flag number (1–4).</summary>
    public int Number { get; }

    public FlagFloor(int number)
    {
        if (number is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(number), number,
                "Flag number must be 1–4.");
        Number = number;
    }
}

public record StartFloor : CellFloor
{
    /// <summary>Starting position index (1–8).</summary>
    public int Index { get; }

    public StartFloor(int index)
    {
        if (index is < 1 or > 8)
            throw new ArgumentOutOfRangeException(nameof(index), index,
                "Start index must be 1–8.");
        Index = index;
    }
}
