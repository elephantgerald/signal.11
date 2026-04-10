namespace Signal11.Domain;

/// <summary>
/// Immutable representation of one board cell, decoded from the 16-bit
/// cell word and its parallel wall byte.
/// </summary>
public record Cell(
    CellFloor Floor,
    Direction ConveyorDirection,
    bool IsExpress,
    int Gear,
    WallSide Walls,
    WallSide WallLasers
);
