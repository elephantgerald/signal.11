using Signal11.Domain;

namespace Signal11.Domain.Integration.Tests;

/// <summary>
/// Round-trip tests: build → encode → parse → compare.
/// These span three components (BoardBuilder, BoardEncoder, BoardParser)
/// and serve as the primary correctness proof for the board I/O pipeline.
/// </summary>
public class BoardRoundTripTests
{
    private static Board RoundTrip(Board board)
    {
        using var ms = new MemoryStream();
        BoardEncoder.Encode(board, ms);
        ms.Position = 0;
        return BoardParser.Parse(ms);
    }

    [Fact]
    public void RoundTrip_AllFloorTypes_CellsIdentical()
    {
        var original = new BoardBuilder(6, 1)
            .SetFloor(0, 0, new NormalFloor())
            .SetFloor(0, 1, new PitFloor())
            .SetFloor(0, 2, new RepairFloor())
            .SetFloor(0, 3, new DoubleRepairFloor())
            .SetFloor(0, 4, new FlagFloor(2))
            .SetFloor(0, 5, new StartFloor(4))
            .Build();

        var parsed = RoundTrip(original);

        Assert.Equal(original.Width,  parsed.Width);
        Assert.Equal(original.Height, parsed.Height);
        for (int col = 0; col < original.Width; col++)
            Assert.Equal(original[0, col], parsed[0, col]);
    }

    [Fact]
    public void RoundTrip_ConveyorAndWalls_CellsIdentical()
    {
        var original = new BoardBuilder(3, 1)
            .SetConveyor(0, 0, Direction.North, isExpress: true)
            .SetConveyor(0, 1, Direction.West,  isExpress: false)
            .SetGear(0, 2, 1)
            .SetWalls(0, 0, WallSide.North | WallSide.East)
            .SetLasers(0, 1, WallSide.South)
            .Build();

        var parsed = RoundTrip(original);

        for (int col = 0; col < original.Width; col++)
            Assert.Equal(original[0, col], parsed[0, col]);
    }

    [Fact]
    public void RoundTrip_FlagPositions_Preserved()
    {
        var original = new BoardBuilder(4, 4)
            .AddFlag(1, 2)
            .AddFlag(3, 0)
            .Build();

        var parsed = RoundTrip(original);

        Assert.Equal(2, parsed.Flags.Count);
        Assert.Equal((1, 2), parsed.Flags[0]);
        Assert.Equal((3, 0), parsed.Flags[1]);
    }
}
