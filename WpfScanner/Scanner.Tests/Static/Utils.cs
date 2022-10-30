namespace Scanner.Tests.Static;

internal static class Utils
{
    public static void RemoveDir(string path)
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