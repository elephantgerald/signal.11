namespace Signal11.Domain;

public class InvalidBoardException : Exception
{
    public InvalidBoardException(string message) : base(message) { }
    public InvalidBoardException(string message, Exception inner) : base(message, inner) { }
}
