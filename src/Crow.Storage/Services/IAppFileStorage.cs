namespace Crow.Services;

/// Writable app-private storage. On iOS and Android this resolves to the app sandbox; on Windows and macOS it uses the per-user application data directory.
public interface IAppFileStorage
{
    /// Root directory for this app's private files.
    string RootDirectory { get; }

    /// Creates parent directories as needed and writes UTF-8 text to a path under <see cref="RootDirectory"/>.
    Task WriteTextAsync(string relativePath, string contents, CancellationToken cancellationToken = default);
}
