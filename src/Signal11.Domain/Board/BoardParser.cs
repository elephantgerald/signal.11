namespace Signal11.Domain;

public static class BoardParser
{
    private const uint Magic = 0x534E3131; // "SN11"
    private const byte SupportedVersion = 1;

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
            byte hi = reader.ReadByte();
            byte lo = reader.ReadByte();
            cellWords[i] = (ushort)((hi << 8) | lo);
        }

        // Wall block — one byte per cell, parallel to data block
        var wallBytes = new byte[cellCount];
        for (int i = 0; i < cellCount; i++)
            wallBytes[i] = reader.ReadByte();

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
        uint magic = ((uint)reader.ReadByte() << 24)
                   | ((uint)reader.ReadByte() << 16)
                   | ((uint)reader.ReadByte() << 8)
                   |  (uint)reader.ReadByte();

        if (magic != Magic)
            throw new InvalidBoardException(
                $"Bad magic bytes 0x{magic:X8}; expected 0x{Magic:X8}.");

        byte version = reader.ReadByte();
        if (version != SupportedVersion)
            throw new UnsupportedBoardVersionException(version);

        width  = reader.ReadByte();
        height = reader.ReadByte();

        byte flagCount = reader.ReadByte();
        flags = new (int, int)[flagCount];
        for (int i = 0; i < flagCount; i++)
            flags[i] = (reader.ReadByte(), reader.ReadByte());
    }

    private static Cell DecodeCell(ushort word, byte wallByte)
    {
        int floorRaw         = (word >> 12) & 0xF;  // bits 15-12
        int conveyorDirRaw   = (word >> 9)  & 0x7;  // bits 11-9
        bool isExpress       = ((word >> 8) & 0x1) == 1; // bit 8
        int gear             = (word >> 6)  & 0x3;  // bits 7-6

        FloorType floor;
        int? flagNumber = null;
        int? startIndex = null;

        if (floorRaw >= 8)
        {
            floor = FloorType.Start;
            startIndex = floorRaw - 7; // 8→1, 9→2, …, 15→8
        }
        else if (floorRaw >= 4)
        {
            floor = FloorType.Flag;
            flagNumber = floorRaw - 3; // 4→1, 5→2, 6→3, 7→4
        }
        else
        {
            floor = (FloorType)floorRaw;
        }

        var walls  = DecodeWallSide(wallByte >> 4); // bits 7-4
        var lasers = DecodeWallSide(wallByte & 0xF); // bits 3-0

        return new Cell(
            floor,
            flagNumber,
            startIndex,
            (Direction)conveyorDirRaw,
            isExpress,
            gear,
            walls,
            lasers
        );
    }

    // Bit layout for both walls and lasers nibble:
    //   bit 3 = N,  bit 2 = E,  bit 1 = S,  bit 0 = W
    private static WallSide DecodeWallSide(int nibble)
    {
        var side = WallSide.None;
        if ((nibble & 0x8) != 0) side |= WallSide.North;
        if ((nibble & 0x4) != 0) side |= WallSide.East;
        if ((nibble & 0x2) != 0) side |= WallSide.South;
        if ((nibble & 0x1) != 0) side |= WallSide.West;
        return side;
    }
}
