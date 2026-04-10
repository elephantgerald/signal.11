using Signal11.Domain;

namespace Signal11.Domain.Tests;

public class BoardBuilderTests
{
    [Fact]
    public void Build_DefaultBoard_AllNormalFloorNoConveyorNoWalls()
    {
        var board = new BoardBuilder(2, 2).Build();

        for (int row = 0; row < 2; row++)
        for (int col = 0; col < 2; col++)
        {
            var cell = board[row, col];
            Assert.IsType<NormalFloor>(cell.Floor);
            Assert.Equal(Direction.None, cell.ConveyorDirection);
            Assert.False(cell.IsExpress);
            Assert.Equal(0, cell.Gear);
            Assert.Equal(WallSide.None, cell.Walls);
            Assert.Equal(WallSide.None, cell.WallLasers);
        }
    }

    [Fact]
    public void SetFloor_PitFloor_StoredCorrectly()
    {
        var board = new BoardBuilder(2, 2).SetFloor(1, 0, new PitFloor()).Build();
        Assert.IsType<PitFloor>(board[1, 0].Floor);
        Assert.IsType<NormalFloor>(board[0, 0].Floor); // others unchanged
    }

    [Fact]
    public void SetFloor_RepairFloor_StoredCorrectly()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new RepairFloor()).Build();
        Assert.IsType<RepairFloor>(board[0, 0].Floor);
    }

    [Fact]
    public void SetFloor_DoubleRepairFloor_StoredCorrectly()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new DoubleRepairFloor()).Build();
        Assert.IsType<DoubleRepairFloor>(board[0, 0].Floor);
    }

    [Fact]
    public void SetFloor_FlagFloor_StoredCorrectly()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new FlagFloor(3)).Build();
        var floor = Assert.IsType<FlagFloor>(board[0, 0].Floor);
        Assert.Equal(3, floor.Number);
    }

    [Fact]
    public void SetFloor_StartFloor_StoredCorrectly()
    {
        var board = new BoardBuilder(1, 1).SetFloor(0, 0, new StartFloor(5)).Build();
        var floor = Assert.IsType<StartFloor>(board[0, 0].Floor);
        Assert.Equal(5, floor.Index);
    }

    [Fact]
    public void SetConveyor_EastExpress_StoredCorrectly()
    {
        var board = new BoardBuilder(1, 1).SetConveyor(0, 0, Direction.East, isExpress: true).Build();
        Assert.Equal(Direction.East, board[0, 0].ConveyorDirection);
        Assert.True(board[0, 0].IsExpress);
    }

    [Fact]
    public void SetConveyor_NonExpress_IsExpressFalse()
    {
        var board = new BoardBuilder(1, 1).SetConveyor(0, 0, Direction.South).Build();
        Assert.Equal(Direction.South, board[0, 0].ConveyorDirection);
        Assert.False(board[0, 0].IsExpress);
    }

    [Fact]
    public void SetGear_StoredCorrectly()
    {
        var board = new BoardBuilder(1, 1).SetGear(0, 0, 2).Build();
        Assert.Equal(2, board[0, 0].Gear);
    }

    [Fact]
    public void SetWalls_StoredCorrectly()
    {
        var board = new BoardBuilder(1, 1)
            .SetWalls(0, 0, WallSide.North | WallSide.East)
            .Build();
        Assert.True(board[0, 0].Walls.HasFlag(WallSide.North));
        Assert.True(board[0, 0].Walls.HasFlag(WallSide.East));
        Assert.False(board[0, 0].Walls.HasFlag(WallSide.South));
        Assert.Equal(WallSide.None, board[0, 0].WallLasers); // lasers unaffected
    }

    [Fact]
    public void SetLasers_StoredCorrectly()
    {
        var board = new BoardBuilder(1, 1)
            .SetLasers(0, 0, WallSide.South | WallSide.West)
            .Build();
        Assert.True(board[0, 0].WallLasers.HasFlag(WallSide.South));
        Assert.True(board[0, 0].WallLasers.HasFlag(WallSide.West));
        Assert.Equal(WallSide.None, board[0, 0].Walls); // walls unaffected
    }

    [Fact]
    public void AddFlag_AppearsOnBuiltBoard()
    {
        var board = new BoardBuilder(4, 4).AddFlag(2, 3).AddFlag(0, 1).Build();
        Assert.Equal(2, board.Flags.Count);
        Assert.Equal((2, 3), board.Flags[0]);
        Assert.Equal((0, 1), board.Flags[1]);
    }

    [Fact]
    public void AddFlag_FiveFlags_ThrowsArgumentException()
    {
        var builder = new BoardBuilder(4, 4)
            .AddFlag(0, 0)
            .AddFlag(1, 0)
            .AddFlag(2, 0)
            .AddFlag(3, 0);
        Assert.Throws<ArgumentException>(() => builder.AddFlag(0, 1));
    }

    [Fact]
    public void AddFlag_OutOfBounds_ThrowsArgumentOutOfRangeException()
    {
        var builder = new BoardBuilder(4, 4);
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddFlag(5, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.AddFlag(0, 9));
    }
}
