namespace Scanner.Tests.Static;

internal static class FileUtils
{
    public static void RemoveDirectory(string path)
    {
        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    public static void MakeFile(string path, int size)
    {
        using var fileStream = new FileStream(path, FileMode.Create);
        fileStream.Write(new byte[size]);
    }
}