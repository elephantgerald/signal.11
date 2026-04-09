namespace Signal11.Domain;

/// <summary>
/// Which sides of a cell have a wall or laser, mirroring the bit layout
/// of the wall byte (bits 7–4 = walls, bits 3–0 = lasers).
/// </summary>
[Flags]
public enum WallSide
{
    None  = 0,
    North = 1 << 0,
    East  = 1 << 1,
    South = 1 << 2,
    West  = 1 << 3,
}
