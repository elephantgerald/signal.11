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
        Assert.IsType<NormalFloor>(board[0, 0].Floor);
    }

    [Fact]
    public void Parse_FloorTypePit_DecodedCorrectly()
    {
        // bits 15-12 = 1  →  0001_xxxx_xxxx_xxxx = 0x1000
        var board = Parse(BuildBoard(1, 1, cellWords: [0x1000]));
        Assert.IsType<PitFloor>(board[0, 0].Floor);
    }

    [Fact]
    public void Parse_FloorTypeRepair_DecodedCorrectly()
    {
        // bits 15-12 = 2  →  0010_0000_0000_0000 = 0x2000
        var board = Parse(BuildBoard(1, 1, cellWords: [0x2000]));
        Assert.IsType<RepairFloor>(board[0, 0].Floor);
    }

    [Fact]
    public void Parse_FloorTypeDoubleRepair_DecodedCorrectly()
    {
        // bits 15-12 = 3  →  0011_0000_0000_0000 = 0x3000
        var board = Parse(BuildBoard(1, 1, cellWords: [0x3000]));
        Assert.IsType<DoubleRepairFloor>(board[0, 0].Floor);
    }

    [Fact]
    public void Parse_FlagFloor_DecodesFloorAndFlagNumber()
    {
        // bits 15-12 = 4  →  0100_0000_0000_0000 = 0x4000  → Flag 1
        var board = Parse(BuildBoard(1, 1, cellWords: [0x4000]));
        var floor = Assert.IsType<FlagFloor>(board[0, 0].Floor);
        Assert.Equal(1, floor.Number);
    }

    [Fact]
    public void Parse_FlagFloor4_DecodesFlag4()
    {
        // bits 15-12 = 7  → Flag 4
        var board = Parse(BuildBoard(1, 1, cellWords: [0x7000]));
        var floor = Assert.IsType<FlagFloor>(board[0, 0].Floor);
        Assert.Equal(4, floor.Number);
    }

    [Fact]
    public void Parse_StartFloor_DecodesFloorAndStartIndex()
    {
        // bits 15-12 = 8  →  1000_0000_0000_0000 = 0x8000  → Start 1
        var board = Parse(BuildBoard(1, 1, cellWords: [0x8000]));
        var floor = Assert.IsType<StartFloor>(board[0, 0].Floor);
        Assert.Equal(1, floor.Index);
    }

    [Fact]
    public void Parse_StartFloor8_DecodesStartIndex8()
    {
        // bits 15-12 = 15  →  1111_0000_0000_0000 = 0xF000  → Start 8
        var board = Parse(BuildBoard(1, 1, cellWords: [0xF000]));
        var floor = Assert.IsType<StartFloor>(board[0, 0].Floor);
        Assert.Equal(8, floor.Index);
    }

    [Fact]
    public void Parse_ConveyorDirectionAndSpeed_DecodedCorrectly()
    {
        // bits 11-9 = 2 (East), bit 8 = 1 (express)
        // 0000_0101_0000_0000 = 0x0500
        var board = Parse(BuildBoard(1, 1, cellWords: [0x0500]));
        Assert.Equal(Direction.East, board[0, 0].ConveyorDirection);
        Assert.True(board[0, 0].IsExpress);
    }

    [Fact]
    public void Parse_ConveyorNotExpress_IsExpressFalse()
    {
        // bits 11-9 = 1 (North), bit 8 = 0
        // 0000_0010_0000_0000 = 0x0200
        var board = Parse(BuildBoard(1, 1, cellWords: [0x0200]));
        Assert.Equal(Direction.North, board[0, 0].ConveyorDirection);
        Assert.False(board[0, 0].IsExpress);
    }

    [Fact]
    public void Parse_ConveyorDirectionSouth_DecodedCorrectly()
    {
        // bits 11-9 = 3 (South)  →  0000_0110_0000_0000 = 0x0600
        var board = Parse(BuildBoard(1, 1, cellWords: [0x0600]));
        Assert.Equal(Direction.South, board[0, 0].ConveyorDirection);
    }

    [Fact]
    public void Parse_ConveyorDirectionWest_DecodedCorrectly()
    {
        // bits 11-9 = 4 (West)  →  0000_1000_0000_0000 = 0x0800
        var board = Parse(BuildBoard(1, 1, cellWords: [0x0800]));
        Assert.Equal(Direction.West, board[0, 0].ConveyorDirection);
    }

    [Fact]
    public void Parse_ConveyorDirectionNone_DecodedCorrectly()
    {
        // bits 11-9 = 0 (None)  →  0x0000
        var board = Parse(BuildBoard(1, 1, cellWords: [0x0000]));
        Assert.Equal(Direction.None, board[0, 0].ConveyorDirection);
    }

    [Fact]
    public void Parse_GearBits_DecodedCorrectly()
    {
        // bits 7-6 = 1  →  0000_0000_0100_0000 = 0x0040
        var board = Parse(BuildBoard(1, 1, cellWords: [0x0040]));
        Assert.Equal(1, board[0, 0].Gear);
    }

    [Fact]
    public void Parse_WallBits_North_South_DecodedCorrectly()
    {
        // wall byte bits 7-4 = 0b1010 (N+S)  →  0xA0
        var board = Parse(BuildBoard(1, 1, wallBytes: [0xA0]));
        Assert.True(board[0, 0].Walls.HasFlag(WallSide.North));
        Assert.True(board[0, 0].Walls.HasFlag(WallSide.South));
        Assert.False(board[0, 0].Walls.HasFlag(WallSide.East));
        Assert.False(board[0, 0].Walls.HasFlag(WallSide.West));
    }

    [Fact]
    public void Parse_WallBits_East_West_DecodedCorrectly()
    {
        // wall byte bits 7-4 = 0b0101 (E+W)  →  0x50
        var board = Parse(BuildBoard(1, 1, wallBytes: [0x50]));
        Assert.True(board[0, 0].Walls.HasFlag(WallSide.East));
        Assert.True(board[0, 0].Walls.HasFlag(WallSide.West));
        Assert.False(board[0, 0].Walls.HasFlag(WallSide.North));
        Assert.False(board[0, 0].Walls.HasFlag(WallSide.South));
    }

    [Fact]
    public void Parse_LaserBits_North_DecodedCorrectly()
    {
        // wall byte bits 3-0 = 0b1000 (N laser)  →  0x08
        var board = Parse(BuildBoard(1, 1, wallBytes: [0x08]));
        Assert.True(board[0, 0].WallLasers.HasFlag(WallSide.North));
        Assert.False(board[0, 0].WallLasers.HasFlag(WallSide.East));
    }

    [Fact]
    public void Parse_LaserBits_AllFour_DecodedCorrectly()
    {
        // wall byte bits 3-0 = 0b1111 (all lasers)  →  0x0F
        var board = Parse(BuildBoard(1, 1, wallBytes: [0x0F]));
        Assert.True(board[0, 0].WallLasers.HasFlag(WallSide.North));
        Assert.True(board[0, 0].WallLasers.HasFlag(WallSide.East));
        Assert.True(board[0, 0].WallLasers.HasFlag(WallSide.South));
        Assert.True(board[0, 0].WallLasers.HasFlag(WallSide.West));
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

    [Fact]
    public void Parse_MultiCellBoard_CellsPlacedAtCorrectRowCol()
    {
        // 2×2 board; place a Pit floor at row=1, col=0 only (index 2 in row-major)
        // 0x1000 = Pit, 0x0000 = Normal
        var cellWords = new ushort[] { 0x0000, 0x0000, 0x1000, 0x0000 };
        var board = Parse(BuildBoard(2, 2, cellWords: cellWords));
        Assert.IsType<NormalFloor>(board[0, 0].Floor);
        Assert.IsType<NormalFloor>(board[0, 1].Floor);
        Assert.IsType<PitFloor>(board[1, 0].Floor);
        Assert.IsType<NormalFloor>(board[1, 1].Floor);
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

    [Fact]
    public void Parse_WidthZero_ThrowsInvalidBoardException()
    {
        var bad = BuildBoard(2, 2);
        bad[5] = 0; // width byte = 0
        Assert.Throws<InvalidBoardException>(() => Parse(bad));
    }

    [Fact]
    public void Parse_HeightZero_ThrowsInvalidBoardException()
    {
        var bad = BuildBoard(2, 2);
        bad[6] = 0; // height byte = 0
        Assert.Throws<InvalidBoardException>(() => Parse(bad));
    }

    [Fact]
    public void Parse_FlagCountExceedsMaximum_ThrowsInvalidBoardException()
    {
        var bad = BuildBoard(2, 2);
        bad[7] = 5; // flagCount byte = 5 (exceeds MaxFlags=4)
        Assert.Throws<InvalidBoardException>(() => Parse(bad));
    }

    [Fact]
    public void Parse_FlagOutOfBounds_ThrowsInvalidBoardException()
    {
        // 2×2 board; flag at (3,3) is out of bounds
        // Build a fresh board with one in-bounds flag, then corrupt the position
        var data = BuildBoard(2, 2, flagPositions: [(0, 0)]);
        data[8]  = 3; // flag X = 3  (>= width 2)
        data[9]  = 3; // flag Y = 3  (>= height 2)
        Assert.Throws<InvalidBoardException>(() => Parse(data));
    }

    [Fact]
    public void Parse_TruncatedStream_ThrowsInvalidBoardException()
    {
        // Truncate after the header — cell data is missing
        var full = BuildBoard(2, 2);
        var truncated = full[..8]; // header only (magic+ver+w+h+flagCount)
        Assert.Throws<InvalidBoardException>(() => Parse(truncated));
    }

    [Fact]
    public void Parse_UndefinedConveyorDirection_ThrowsInvalidBoardException()
    {
        // bits 11-9 = 5 (undefined)  →  0000_1010_0000_0000 = 0x0A00
        var bad = BuildBoard(1, 1, cellWords: [0x0A00]);
        Assert.Throws<InvalidBoardException>(() => Parse(bad));
    }

    [Fact]
    public void Parse_ReservedBitsNonZero_ThrowsInvalidBoardException()
    {
        // bit 0 set in reserved range (bits 5–0)  →  0x0001
        var bad = BuildBoard(1, 1, cellWords: [0x0001]);
        Assert.Throws<InvalidBoardException>(() => Parse(bad));
    }

    [Fact]
    public void Parse_GearReservedValue_ThrowsInvalidBoardException()
    {
        // bits 7–6 = 11 (gear = 3, reserved)  →  0x00C0
        var bad = BuildBoard(1, 1, cellWords: [0x00C0]);
        Assert.Throws<InvalidBoardException>(() => Parse(bad));
    }
}
