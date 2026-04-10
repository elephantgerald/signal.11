namespace Signal11.Domain;

/// <summary>
/// Writes a <see cref="Board"/> to a stream in the SN11 binary format.
/// Inverse of <see cref="BoardParser"/>.
/// </summary>
public static class BoardEncoder
{
    private const byte Version = 1;

    public static void Encode(Board board, Stream stream)
    {
        using var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, leaveOpen: true);

        WriteHeader(writer, board);
        WriteDataBlock(writer, board);
        WriteWallBlock(writer, board);
    }

    private static void WriteHeader(BinaryWriter writer, Board board)
    {
        // Magic "SN11" — 4 bytes big-endian
        writer.Write((byte)0x53);
        writer.Write((byte)0x4E);
        writer.Write((byte)0x31);
        writer.Write((byte)0x31);

        writer.Write(Version);
        writer.Write((byte)board.Width);
        writer.Write((byte)board.Height);
        writer.Write((byte)board.Flags.Count);

        foreach (var (x, y) in board.Flags)
        {
            writer.Write((byte)x);
            writer.Write((byte)y);
        }
    }

    private static void WriteDataBlock(BinaryWriter writer, Board board)
    {
        for (int row = 0; row < board.Height; row++)
        for (int col = 0; col < board.Width;  col++)
        {
            ushort word = EncodeCellWord(board[row, col]);
            writer.Write((byte)(word >> 8));
            writer.Write((byte)(word & 0xFF));
        }
    }

    private static void WriteWallBlock(BinaryWriter writer, Board board)
    {
        for (int row = 0; row < board.Height; row++)
        for (int col = 0; col < board.Width;  col++)
        {
            var cell = board[row, col];
            writer.Write((byte)(((int)cell.Walls << 4) | (int)cell.WallLasers));
        }
    }

    private static ushort EncodeCellWord(Cell cell)
    {
        int floorRaw = cell.Floor switch
        {
            NormalFloor       => 0,
            PitFloor          => 1,
            RepairFloor       => 2,
            DoubleRepairFloor => 3,
            FlagFloor f       => f.Number + 3,   // Flag 1→4, Flag 4→7
            StartFloor s      => s.Index  + 7,   // Start 1→8, Start 8→15
            _                 => throw new ArgumentException($"Unknown floor type {cell.Floor.GetType().Name}.")
        };

        return (ushort)(
            (floorRaw                      << 12) |
            ((int)cell.ConveyorDirection   <<  9) |
            (cell.IsExpress ? 0x100 : 0)          |
            (cell.Gear                     <<  6)
        );
    }
}
