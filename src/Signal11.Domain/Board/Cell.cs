namespace Signal11.Domain;

/// <summary>
/// Immutable representation of one board cell, decoded from the 16-bit
/// cell word and its parallel wall byte.
/// </summary>
public record Cell(
    FloorType Floor,
    int? FlagNumber,        // 1–4, set when Floor == FloorType.Flag
    int? StartIndex,        // 1–8, set when Floor == FloorType.Start
    Direction ConveyorDirection,
    bool IsExpress,
    int Gear,
    WallSide Walls,
    WallSide WallLasers
);
