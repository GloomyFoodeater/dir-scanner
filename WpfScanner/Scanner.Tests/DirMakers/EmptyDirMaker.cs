using Scanner.Core.Models;
using Scanner.Tests.Interfaces;
using static Scanner.Tests.Static.FileUtils;

namespace Scanner.Tests.DirMakers;

internal class EmptyDirMaker : IDirMaker
{
    public FileTree Create(string destinationPath)
    {
        destinationPath = Path.GetFullPath(destinationPath);

        RemoveDirectory(destinationPath);
        Directory.CreateDirectory(destinationPath);

        return new FileTree(destinationPath, new List<FileNode>());
    }
}