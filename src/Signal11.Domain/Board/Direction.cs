namespace Signal11.Domain;

/// <summary>Conveyor belt direction. Encoded in bits 11–9 of the cell word.</summary>
public enum Direction
{
    None = 0,
    North = 1,
    East = 2,
    South = 3,
    West = 4,
}
