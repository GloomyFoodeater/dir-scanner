using Scanner.Core.Models;
using Scanner.Tests.Interfaces;
using static Scanner.Tests.Static.Utils;

namespace Scanner.Tests.DirMakers;

internal class ManyFilesDirMaker : IDirMaker
{
    public FileTree Create(string destinationPath)
    {
        destinationPath = Path.GetFullPath(destinationPath);
        
        RemoveDir(destinationPath);
        Directory.CreateDirectory(destinationPath);

        List<FileNode> children = new();
        
        // Generating files & appending to children list.
        const int fileCount = 50;
        const int startSize = 50;
        for (int i = 0; i < fileCount; i++)
        {
            var filePath = $"{destinationPath}\\{i}.txt";
            var fileSize = i * startSize;
            MakeFile(filePath, fileSize);
            
            children.Add(new(filePath, fileSize));
        }

        FileTree res = new(destinationPath, children);
        res.RecalculateSize();
        return res;
    }
}