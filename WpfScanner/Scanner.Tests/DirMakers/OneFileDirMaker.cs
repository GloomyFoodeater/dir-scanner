using Scanner.Core.Models;
using Scanner.Tests.Interfaces;
using static Scanner.Tests.Static.FileUtils;

namespace Scanner.Tests.DirMakers;

internal class OneFileDirMaker : IDirMaker
{
    public FileTree Create(string destinationPath)
    {
        destinationPath = Path.GetFullPath(destinationPath);
        var size = 100;
        var filePath = $"{destinationPath}\\1.txt";

        RemoveDirectory(destinationPath);
        Directory.CreateDirectory(destinationPath);
        MakeFile(filePath, size);

        FileTree res = new(destinationPath, new List<FileNode> { new(filePath, size) });
        res.RecalculateSize();
        return res;
    }
}