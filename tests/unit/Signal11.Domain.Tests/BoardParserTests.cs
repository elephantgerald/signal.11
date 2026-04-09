using Signal11.Domain;

namespace Signal11.Domain.Tests;

public class BoardParserTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a minimal well-formed SN11 binary for a Width×Height board.
    /// All cells default to 0x0000 (Normal floor, no conveyor, no walls).
    /// flagPositions is a flat list of (x,y) pairs matching flagCount.
    /// cellWords and wallBytes are indexed row-major [row*width + col].
    /// </summary>
    private static byte[] BuildBoard(
        byte width, byte height,
        ushort[]? cellWords = null,
        byte[]? wallBytes = null,
        (byte x, byte y)[]? flagPositions = null)
    {
        var flags = flagPositions ?? [];
        int cellCount = width * height;

        var bytes = new List<byte>();

        // Header
        bytes.AddRange(new byte[] { 0x53, 0x4E, 0x31, 0x31 }); // "SN11"
        bytes.Add(1);           // version
        bytes.Add(width);
        bytes.Add(height);
        bytes.Add((byte)flags.Length);

        foreach (var (x, y) in flags)
        {
            bytes.Add(x);
            bytes.Add(y);
        }

        // Data block — 16-bit cell words, big-endian
        for (int i = 0; i < cellCount; i++)
        {
            ushort word = cellWords != null ? cellWords[i] : (ushort)0;
            bytes.Add((byte)(word >> 8));
            bytes.Add((byte)(word & 0xFF));
        }

        // Wall block
        for (int i = 0; i < cellCount; i++)
        {
            bytes.Add(wallBytes != null ? wallBytes[i] : (byte)0);
        }

        return bytes.ToArray();
    }

    private static Board Parse(byte[] data) =>
        BoardParser.Parse(new MemoryStream(data));

    // ── happy-path ───────────────────────────────────────────────────────────

    [Fact]
    public void Parse_WellFormedBinary_ReturnsCorrectDimensions()
    {
        var board = Parse(BuildBoard(4, 6));
        Assert.Equal(4, board.Width);
        Assert.Equal(6, board.Height);
    }

    [Fact]
    public void Parse_CellWordZero_NormalFloor()
    {
        var board = Parse(BuildBoard(1, 1, cellWords: [0x0000]));
        Assert.Equal(FloorType.Normal, board.Cells[0, 0].Floor);
        Assert.Null(board.Cells[0, 0].FlagNumber);
        Assert.Null(board.Cells[0, 0].StartIndex);
    }

    [Fact]
    public void Parse_FloorTypePit_DecodedCorrectly()
    {
        // bits 15-12 = 1  →  0001_xxxx_xxxx_xxxx = 0x1000
        var board = Parse(BuildBoard(1, 1, cellWords: [0x1000]));
        Assert.Equal(FloorType.Pit, board.Cells[0, 0].Floor);
    }

    [Fact]
    public void Parse_FlagFloor_DecodesFloorAndFlagNumber()
    {
        // bits 15-12 = 4  →  0100_0000_0000_0000 = 0x4000  → Flag 1
        var board = Parse(BuildBoard(1, 1, cellWords: [0x4000]));
        Assert.Equal(FloorType.Flag, board.Cells[0, 0].Floor);
        Assert.Equal(1, board.Cells[0, 0].FlagNumber);
    }

    [Fact]
    public void Parse_FlagFloor4_DecodesFlag4()
    {
        // bits 15-12 = 7  → Flag 4
        var board = Parse(BuildBoard(1, 1, cellWords: [0x7000]));
        Assert.Equal(FloorType.Flag, board.Cells[0, 0].Floor);
        Assert.Equal(4, board.Cells[0, 0].FlagNumber);
    }

    [Fact]
    public void Parse_StartFloor_DecodesFloorAndStartIndex()
    {
        // bits 15-12 = 8  →  1000_0000_0000_0000 = 0x8000  → Start 1
        var board = Parse(BuildBoard(1, 1, cellWords: [0x8000]));
        Assert.Equal(FloorType.Start, board.Cells[0, 0].Floor);
        Assert.Equal(1, board.Cells[0, 0].StartIndex);
    }

    [Fact]
    public void Parse_ConveyorDirectionAndSpeed_DecodedCorrectly()
    {
        // bits 11-9 = 2 (East), bit 8 = 1 (express)
        // 0000_0101_0000_0000 = 0x0500
        var board = Parse(BuildBoard(1, 1, cellWords: [0x0500]));
        Assert.Equal(Direction.East, board.Cells[0, 0].ConveyorDirection);
        Assert.True(board.Cells[0, 0].IsExpress);
    }

    [Fact]
    public void Parse_ConveyorNotExpress_IsExpressFalse()
    {
        // bits 11-9 = 1 (North), bit 8 = 0
        // 0000_0010_0000_0000 = 0x0200
        var board = Parse(BuildBoard(1, 1, cellWords: [0x0200]));
        Assert.Equal(Direction.North, board.Cells[0, 0].ConveyorDirection);
        Assert.False(board.Cells[0, 0].IsExpress);
    }

    [Fact]
    public void Parse_GearBits_DecodedCorrectly()
    {
        // bits 7-6 = 1  →  0000_0000_0100_0000 = 0x0040
        var board = Parse(BuildBoard(1, 1, cellWords: [0x0040]));
        Assert.Equal(1, board.Cells[0, 0].Gear);
    }

    [Fact]
    public void Parse_WallBits_DecodedCorrectly()
    {
        // wall byte 0b1010_0000 = 0xA0  →  N wall (bit 7) + S wall (bit 5)
        var board = Parse(BuildBoard(1, 1, wallBytes: [0xA0]));
        Assert.True(board.Cells[0, 0].Walls.HasFlag(WallSide.North));
        Assert.True(board.Cells[0, 0].Walls.HasFlag(WallSide.South));
        Assert.False(board.Cells[0, 0].Walls.HasFlag(WallSide.East));
        Assert.False(board.Cells[0, 0].Walls.HasFlag(WallSide.West));
    }

    [Fact]
    public void Parse_LaserBits_DecodedCorrectly()
    {
        // wall byte 0b0000_1000 = 0x08  →  N wall laser (bit 3)
        var board = Parse(BuildBoard(1, 1, wallBytes: [0x08]));
        Assert.True(board.Cells[0, 0].WallLasers.HasFlag(WallSide.North));
        Assert.False(board.Cells[0, 0].WallLasers.HasFlag(WallSide.East));
    }

    [Fact]
    public void Parse_FlagPositions_ParsedCorrectly()
    {
        var board = Parse(BuildBoard(4, 4,
            flagPositions: [(2, 3)]));
        Assert.Single(board.Flags);
        Assert.Equal((2, 3), board.Flags[0]);
    }

    [Fact]
    public void Parse_NoFlags_EmptyFlagsArray()
    {
        var board = Parse(BuildBoard(2, 2));
        Assert.Empty(board.Flags);
    }

    // ── error cases ──────────────────────────────────────────────────────────

    [Fact]
    public void Parse_BadMagic_ThrowsInvalidBoardException()
    {
        var bad = BuildBoard(2, 2);
        bad[0] = 0x00; // corrupt magic
        Assert.Throws<InvalidBoardException>(() => Parse(bad));
    }

    [Fact]
    public void Parse_UnknownVersion_ThrowsUnsupportedBoardVersionException()
    {
        var bad = BuildBoard(2, 2);
        bad[4] = 0xFF; // corrupt version
        var ex = Assert.Throws<UnsupportedBoardVersionException>(() => Parse(bad));
        Assert.Equal(0xFF, ex.Version);
    }
}
