namespace Signal11.Domain;

public static class BoardParser
{
    private const uint Magic            = 0x534E3131; // "SN11"
    private const byte SupportedVersion = 1;
    private const byte MaxFlags         = 4;

    public static Board Parse(Stream stream)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);

        ValidateHeader(reader, out byte width, out byte height, out var flags);

        int cellCount = width * height;
        var cells = new Cell[height, width];

        // Data block — 16-bit cell words, big-endian, row-major
        var cellWords = new ushort[cellCount];
        for (int i = 0; i < cellCount; i++)
        {
            byte hi = ReadField(reader, "cell word (high byte)");
            byte lo = ReadField(reader, "cell word (low byte)");
            cellWords[i] = (ushort)((hi << 8) | lo);
        }

        // Wall block — one byte per cell, parallel to data block
        var wallBytes = new byte[cellCount];
        for (int i = 0; i < cellCount; i++)
            wallBytes[i] = ReadField(reader, "wall byte");

        for (int row = 0; row < height; row++)
        for (int col = 0; col < width; col++)
        {
            int i = row * width + col;
            cells[row, col] = DecodeCell(cellWords[i], wallBytes[i]);
        }

        return new Board(width, height, cells, flags);
    }

    private static void ValidateHeader(
        BinaryReader reader,
        out byte width,
        out byte height,
        out (int X, int Y)[] flags)
    {
        // Magic — 4 bytes big-endian
        uint magic = ((uint)ReadField(reader, "magic byte 0") << 24)
                   | ((uint)ReadField(reader, "magic byte 1") << 16)
                   | ((uint)ReadField(reader, "magic byte 2") << 8)
                   |  (uint)ReadField(reader, "magic byte 3");

        if (magic != Magic)
            throw new InvalidBoardException(
                $"Bad magic bytes 0x{magic:X8}; expected 0x{Magic:X8}.");

        byte version = ReadField(reader, "version");
        if (version != SupportedVersion)
            throw new UnsupportedBoardVersionException(version);

        width  = ReadField(reader, "width");
        height = ReadField(reader, "height");

        if (width  == 0) throw new InvalidBoardException("Board width must be at least 1.");
        if (height == 0) throw new InvalidBoardException("Board height must be at least 1.");

        byte flagCount = ReadField(reader, "flag count");
        if (flagCount > MaxFlags)
            throw new InvalidBoardException(
                $"Flag count {flagCount} exceeds maximum of {MaxFlags}.");

        flags = new (int, int)[flagCount];
        for (int i = 0; i < flagCount; i++)
        {
            byte fx = ReadField(reader, $"flag {i + 1} X");
            byte fy = ReadField(reader, $"flag {i + 1} Y");
            if (fx >= width || fy >= height)
                throw new InvalidBoardException(
                    $"Flag {i + 1} position ({fx},{fy}) is outside board {width}×{height}.");
            flags[i] = (fx, fy);
        }
    }

    private static Cell DecodeCell(ushort word, byte wallByte)
    {
        int floorRaw       = (word >> 12) & 0xF;  // bits 15-12
        int conveyorDirRaw = (word >> 9)  & 0x7;  // bits 11-9
        bool isExpress     = ((word >> 8) & 0x1) == 1; // bit 8
        int gear           = (word >> 6)  & 0x3;  // bits 7-6

        if ((word & 0x3F) != 0)
            throw new InvalidBoardException(
                $"Reserved bits 5–0 are non-zero (word=0x{word:X4}); possible format corruption.");

        if (conveyorDirRaw > 4)
            throw new InvalidBoardException(
                $"Undefined conveyor direction value {conveyorDirRaw}.");

        if (gear == 3)
            throw new InvalidBoardException(
                $"Gear value 3 is reserved and undefined.");

        CellFloor floor = floorRaw switch
        {
            >= 8  => new StartFloor(floorRaw - 7),   // 8→1, 9→2, …, 15→8
            >= 4  => new FlagFloor(floorRaw - 3),     // 4→1, 5→2, 6→3, 7→4
            0     => new NormalFloor(),
            1     => new PitFloor(),
            2     => new RepairFloor(),
            3     => new DoubleRepairFloor(),
            _     => throw new InvalidBoardException($"Unrecognised floor value {floorRaw}.")
        };

        var walls  = (WallSide)(wallByte >> 4); // bits 7-4
        var lasers = (WallSide)(wallByte & 0xF); // bits 3-0

        return new Cell(
            floor,
            (Direction)conveyorDirRaw,
            isExpress,
            gear,
            walls,
            lasers
        );
    }

    /// <summary>
    /// Reads one byte, wrapping <see cref="EndOfStreamException"/> into
    /// <see cref="InvalidBoardException"/> so callers see a consistent error type.
    /// </summary>
    private static byte ReadField(BinaryReader reader, string fieldName)
    {
        try
        {
            return reader.ReadByte();
        }
        catch (EndOfStreamException ex)
        {
            throw new InvalidBoardException(
                $"Board data truncated reading '{fieldName}'.", ex);
        }
    }
}
