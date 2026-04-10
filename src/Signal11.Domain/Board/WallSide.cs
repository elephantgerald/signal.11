namespace Signal11.Domain;

/// <summary>
/// Which sides of a cell have a wall or laser. Values match the SN11 wire
/// nibble: bit 3 = North, bit 2 = East, bit 1 = South, bit 0 = West.
/// </summary>
[Flags]
public enum WallSide
{
    None  = 0,
    North = 1 << 3,
    East  = 1 << 2,
    South = 1 << 1,
    West  = 1 << 0,
}
