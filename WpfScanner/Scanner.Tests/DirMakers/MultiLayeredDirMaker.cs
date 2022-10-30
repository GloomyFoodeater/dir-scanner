using Scanner.Core.Models;
using Scanner.Tests.Interfaces;
using static Scanner.Tests.DirMakers.Utils;

namespace Scanner.Tests.DirMakers;

internal class MultiLayeredDirMaker : IDirMaker
{
    public FileTree MakeDir(int nestLevel, string rootPath)
    {

        List<FileNode> children = new();

        const int startSize = 100;
        for (int i = 0; i < 10; i++)
        {
            string path;
            if (i % 2 == 0)
            {
                path = $"{rootPath}\\{i / 2}.txt";
                MakeFile(path, i * startSize);
                children.Add(new(path, i * startSize));
            }
            else if(nestLevel > 0)
            {
                path = $"{rootPath}\\{i / 2}";
                Directory.CreateDirectory(path);
                var child = MakeDir(nestLevel - 1, path);
                children.Add(child);
            }
        }

        return new FileTree(rootPath, children);
    }
    
    public FileTree Create(string destinationPath)
    {
        destinationPath = Path.GetFullPath(destinationPath);
        
        RemoveDir(destinationPath);
        Directory.CreateDirectory(destinationPath);
        
        return MakeDir(3, destinationPath);
    }
}