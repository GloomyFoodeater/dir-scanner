using Scanner.Core.Models;
using Scanner.Tests.Interfaces;
using static Scanner.Tests.Static.Utils;

namespace Scanner.Tests.DirMakers;

internal class LinksDirMaker : IDirMaker
{
    public FileTree Create(string destinationPath)
    {
        destinationPath = Path.GetFullPath(destinationPath);

        RemoveDir(destinationPath);
        Directory.CreateDirectory(destinationPath);
        MakeFile($@"{destinationPath}\1.txt", 100);

        Directory.CreateSymbolicLink($@"{destinationPath}\{Path.GetFileName(destinationPath)}", destinationPath);
        File.CreateSymbolicLink($@"{destinationPath}\1_lnk.txt", $@"{destinationPath}\1.txt");

        FileTree res = new FileTree(
            destinationPath,
            new List<FileNode> { new($@"{destinationPath}\1.txt", 100) });
        res.RecalculateSize();
        return res;
    }
}