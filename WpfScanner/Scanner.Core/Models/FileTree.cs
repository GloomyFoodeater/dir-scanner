namespace Scanner.Core.Models;

public class FileTree : FileNode
{
    public FileTree(string path, IEnumerable<FileNode> children) : base(path) => Children = children;

    public IEnumerable<FileNode> Children { get; }

    public void RecalculateSize()
    {
        Size = 0;
        foreach (var child in Children)
        {
            // Recursive recalculation.
            if (child is FileTree fileTree)
                fileTree.RecalculateSize();
            Size += child.Size;
        }
    }
}