using Signal11.Domain;

namespace Signal11.Domain.Tests;

public class BoardEncoderTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static byte[] EncodeToBytes(Board board)
    {
        using var ms = new MemoryStream();
        BoardEncoder.Encode(board, ms);
        return ms.ToArray();
    }

    private static Board SimpleBoard(int width = 2, int height = 2) =>
        new BoardBuilder(width, height).Build();

    // Offset of first data-block cell word in the encoded bytes for a board
    // with the given flag count.  Header = 4+1+1+1+1+(flagCount*2) bytes.
    private static int DataBlockOffset(int flagCount = 0) => 8 + flagCount * 2;

    // Reads the big-endian 16-bit cell word at a given cell index in the data block.
    private static ushort ReadCellWord(byte[] bytes, int cellIndex, int flagCount = 0)
    {
        int offset = DataBlockOffset(flagCount) + cellIndex * 2;
        return (ushort)((bytes[offset] << 8) | bytes[offset + 1]);
    }

    // Reads the wall byte for a given cell index (after the full data block).
    private static byte ReadWallByte(byte[] bytes, int cellIndex, int width, int height, int flagCount = 0)
    {
        int wallBlockOffset = DataBlockOffset(flagCount) + width * height * 2;
        return bytes[wallBlockOffset + cellIndex];
    }

    // ── header checks ────────────────────────────────────────────────────────

    [Fact]
    public void Encode_WritesMagic()
    {
        var bytes = EncodeToBytes(SimpleBoard());
        Assert.Equal(0x53, bytes[0]); // 'S'
        Assert.Equal(0x4E, bytes[1]); // 'N'
        Assert.Equal(0x31, bytes[2]); // '1'
        Assert.Equal(0x31, bytes[3]); // '1'
    }

    [Fact]
    public void Encode_WritesVersion1()
    {
        var bytes = EncodeToBytes(SimpleBoard());
        Assert.Equal(1, bytes[4]);
    }

    [Fact]
    public void Encode_WritesDimensions()
    {
        var bytes = EncodeToBytes(SimpleBoard(width: 5, height: 3));
        Assert.Equal(5, bytes[5]); // width
        Assert.Equal(3, bytes[6]); // height
    }

    [Fact]
    public void Encode_FlagCountAndPositions()
    {
        var board = new BoardBuilder(4, 4).AddFlag(2, 3).AddFlag(1, 0).Build();
        var bytes = EncodeToBytes(board);
        Assert.Equal(2,  bytes[7]);  // flagCount
        Assert.Equal(2,  bytes[8]);  // flag 0 X
        Assert.Equal(3,  bytes[9]);  // flag 0 Y
        Assert.Equal(1,  bytes[10]); // flag 1 X
        Assert.Equal(0,  bytes[11]); // flag 1 Y
    }

    // ── cell word checks (1×1 boards) ─────────────────────────────────────────

    [Fact]
    public void Encode_NormalFloor_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).Build(); // default = Normal
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(0, word >> 12); // bits 15-12 = 0
    }

    [Fact]
    public void Encode_PitFloor_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new PitFloor()).Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(1, word >> 12);
    }

    [Fact]
    public void Encode_RepairFloor_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new RepairFloor()).Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(2, word >> 12);
    }

    [Fact]
    public void Encode_DoubleRepairFloor_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new DoubleRepairFloor()).Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(3, word >> 12);
    }

    [Fact]
    public void Encode_FlagFloor1_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new FlagFloor(1)).Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(4, word >> 12); // Flag 1 → raw 4
    }

    [Fact]
    public void Encode_FlagFloor4_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new FlagFloor(4)).Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(7, word >> 12); // Flag 4 → raw 7
    }

    [Fact]
    public void Encode_StartFloor1_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new StartFloor(1)).Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(8, word >> 12); // Start 1 → raw 8
    }

    [Fact]
    public void Encode_StartFloor8_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new StartFloor(8)).Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(15, word >> 12); // Start 8 → raw 15
    }

    [Fact]
    public void Encode_ConveyorEastExpress_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).SetConveyor(0, 0, Direction.East, isExpress: true).Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(2, (word >> 9) & 0x7); // bits 11-9 = East = 2
        Assert.Equal(1, (word >> 8) & 0x1); // bit 8 = express
    }

    [Fact]
    public void Encode_ConveyorNorth_NotExpress_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).SetConveyor(0, 0, Direction.North).Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(1, (word >> 9) & 0x7); // North = 1
        Assert.Equal(0, (word >> 8) & 0x1); // not express
    }

    [Fact]
    public void Encode_Gear1_WordBitsCorrect()
    {
        var board = new BoardBuilder(1, 1).SetGear(0, 0, 1).Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(1, (word >> 6) & 0x3); // bits 7-6 = 1
    }

    [Fact]
    public void Encode_ReservedBitsAlwaysZero()
    {
        var board = new BoardBuilder(1, 1)
            .SetFloor(0, 0, new PitFloor())
            .SetConveyor(0, 0, Direction.West, isExpress: true)
            .SetGear(0, 0, 2)
            .Build();
        var word = ReadCellWord(EncodeToBytes(board), 0);
        Assert.Equal(0, word & 0x3F); // bits 5-0 always 0
    }

    // ── wall byte checks ─────────────────────────────────────────────────────

    [Fact]
    public void Encode_WallsNorthSouth_WallByteCorrect()
    {
        var board = new BoardBuilder(1, 1)
            .SetWalls(0, 0, WallSide.North | WallSide.South)
            .Build();
        byte wb = ReadWallByte(EncodeToBytes(board), 0, 1, 1);
        Assert.Equal(0xA0, wb); // bits 7-4: N+S = 1010
    }

    [Fact]
    public void Encode_LasersEastWest_WallByteCorrect()
    {
        var board = new BoardBuilder(1, 1)
            .SetLasers(0, 0, WallSide.East | WallSide.West)
            .Build();
        byte wb = ReadWallByte(EncodeToBytes(board), 0, 1, 1);
        Assert.Equal(0x05, wb); // bits 3-0: E+W = 0101
    }

    [Fact]
    public void Encode_WallsAndLasers_CombinedWallByteCorrect()
    {
        var board = new BoardBuilder(1, 1)
            .SetWalls(0, 0, WallSide.North)
            .SetLasers(0, 0, WallSide.South)
            .Build();
        byte wb = ReadWallByte(EncodeToBytes(board), 0, 1, 1);
        Assert.Equal(0x82, wb); // N wall (0x80) | S laser (0x02)
    }

    // ── round-trip tests ──────────────────────────────────────────────────────

    [Fact]
    public void RoundTrip_AllFloorTypes_CellsIdentical()
    {
        // One row, one cell per floor type
        var builder = new BoardBuilder(6, 1)
            .SetFloor(0, 0, new NormalFloor())
            .SetFloor(0, 1, new PitFloor())
            .SetFloor(0, 2, new RepairFloor())
            .SetFloor(0, 3, new DoubleRepairFloor())
            .SetFloor(0, 4, new FlagFloor(2))
            .SetFloor(0, 5, new StartFloor(4));

        var original = builder.Build();
        var roundTripped = BoardParser.Parse(new MemoryStream(EncodeToBytes(original)));

        Assert.Equal(original.Width,  roundTripped.Width);
        Assert.Equal(original.Height, roundTripped.Height);

        for (int col = 0; col < original.Width; col++)
            Assert.Equal(original[0, col], roundTripped[0, col]);
    }

    [Fact]
    public void RoundTrip_ConveyorAndWalls_CellsIdentical()
    {
        var builder = new BoardBuilder(3, 1)
            .SetConveyor(0, 0, Direction.North, isExpress: true)
            .SetConveyor(0, 1, Direction.West,  isExpress: false)
            .SetGear(0, 2, 1)
            .SetWalls(0, 0, WallSide.North | WallSide.East)
            .SetLasers(0, 1, WallSide.South);

        var original = builder.Build();
        var roundTripped = BoardParser.Parse(new MemoryStream(EncodeToBytes(original)));

        for (int col = 0; col < original.Width; col++)
            Assert.Equal(original[0, col], roundTripped[0, col]);
    }

    [Fact]
    public void RoundTrip_FlagPositions_Preserved()
    {
        var original = new BoardBuilder(4, 4)
            .AddFlag(1, 2)
            .AddFlag(3, 0)
            .Build();

        var roundTripped = BoardParser.Parse(new MemoryStream(EncodeToBytes(original)));

        Assert.Equal(2, roundTripped.Flags.Count);
        Assert.Equal((1, 2), roundTripped.Flags[0]);
        Assert.Equal((3, 0), roundTripped.Flags[1]);
    }
}
