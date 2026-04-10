namespace Signal11.Domain;

public class UnsupportedBoardVersionException : Exception
{
    public byte Version { get; }

    public UnsupportedBoardVersionException(byte version)
        : base($"Board version {version} is not supported.")
    {
        Version = version;
    }
}
