namespace Metalama.Samples.LegacyTesting1.TestSeams;

// Replaces File.ReadAllText in the Test configuration. The tests add the files in memory.
public static class FakeFileSystem
{
    private static readonly Dictionary<string, string> _files = new();

    public static void AddFile( string path, string contents ) => _files[path] = contents;

    public static string ReadAllText( string path )
        => _files.TryGetValue( path, out var contents )
            ? contents
            : throw new FileNotFoundException( $"The fake file system has no file '{path}'.", path );

    public static void Reset() => _files.Clear();
}
