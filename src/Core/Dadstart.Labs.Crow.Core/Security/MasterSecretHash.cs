namespace Dadstart.Labs.Crow.Security;

public sealed record class MasterSecretHash
{
    public required string Algorithm { get; init; }

    public required byte[] Salt { get; init; }

    public required byte[] Hash { get; init; }

    public int Iterations { get; init; }
}

