﻿using Scanner.Core.Models;
using Scanner.Tests.Interfaces;
using static Scanner.Tests.DirMakers.Utils;

namespace Scanner.Tests.DirMakers;

internal class EmptyDirMaker : IDirMaker
{
    public FileTree Create(string destinationPath)
    {
        destinationPath = Path.GetFullPath(destinationPath);
        
        RemoveDir(destinationPath);
        Directory.CreateDirectory(destinationPath);
        
        return new FileTree(destinationPath, new List<FileNode>());
    }
}