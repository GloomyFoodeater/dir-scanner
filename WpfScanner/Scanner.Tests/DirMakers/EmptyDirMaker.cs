using Scanner.Core.Models;
using Scanner.Tests.Interfaces;

namespace Scanner.Tests.DirMakers;

internal class EmptyDirMaker : IDirMaker
{
    public FileTree Create(string destinationPath)
    {
        destinationPath = Path.GetFullPath(destinationPath);
        if(Directory.Exists(destinationPath))
            Directory.Delete(destinationPath, true);
        
        Directory.CreateDirectory(destinationPath);
        
        return new FileTree(destinationPath, new List<FileNode>());
    }
}