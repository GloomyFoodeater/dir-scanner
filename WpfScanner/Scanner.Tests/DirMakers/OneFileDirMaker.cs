using Scanner.Core.Models;
using Scanner.Tests.Interfaces;
using static Scanner.Tests.Static.Utils;

namespace Scanner.Tests.DirMakers;

internal class OneFileDirMaker : IDirMaker
{
    public FileTree Create(string destinationPath)
    {
        destinationPath = Path.GetFullPath(destinationPath);
        var size = 100;
        var filePath = $"{destinationPath}\\1.txt";
        
        RemoveDir(destinationPath);
        Directory.CreateDirectory(destinationPath);
        MakeFile(filePath, size);
        
        return new FileTree(destinationPath, new List<FileNode> { new (filePath, size) });
    }
}