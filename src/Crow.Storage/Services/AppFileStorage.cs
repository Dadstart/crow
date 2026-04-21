using System.Text;

namespace Crow.Services;

public sealed class AppFileStorage : IAppFileStorage
{
    static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    readonly string _rootDirectory;

    public AppFileStorage(string rootDirectory)
    {
        if (string.IsNullOrWhiteSpace(rootDirectory))
            throw new ArgumentException("Root directory is required.", nameof(rootDirectory));

        _rootDirectory = Path.TrimEndingDirectorySeparator(Path.GetFullPath(rootDirectory));
    }

    public string RootDirectory => _rootDirectory;

    public async Task WriteTextAsync(string relativePath, string contents, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(relativePath);
        ArgumentNullException.ThrowIfNull(contents);

        var normalizedRelative = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        if (Path.IsPathRooted(normalizedRelative))
            throw new ArgumentException("Relative path only.", nameof(relativePath));

        var fullPath = Path.GetFullPath(Path.Combine(_rootDirectory, normalizedRelative));
        if (!IsUnderRoot(fullPath))
            throw new ArgumentException("Path escapes storage root.", nameof(relativePath));

        var parent = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(parent))
            Directory.CreateDirectory(parent);

        await File.WriteAllTextAsync(fullPath, contents, Utf8NoBom, cancellationToken).ConfigureAwait(false);
    }

    bool IsUnderRoot(string fullPath)
    {
        var full = Path.TrimEndingDirectorySeparator(Path.GetFullPath(fullPath));
        var root = _rootDirectory;
        if (full.Equals(root, PathInternal.StringComparison))
            return true;

        var prefix = root + Path.DirectorySeparatorChar;
        return full.StartsWith(prefix, PathInternal.StringComparison);
    }

    static class PathInternal
    {
        internal static readonly StringComparison StringComparison =
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    }
}
