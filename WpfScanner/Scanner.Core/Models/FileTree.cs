namespace Scanner.Core.Models;

public class FileTree : FileNode
{
    public FileTree(string name, IList<FileNode> children) : base(name, 0) => Children = children;

    public IList<FileNode> Children { get; }

    public void RecalculateSize()
    {
        Size = 0;
        foreach (var child in Children)
        {
            if (child is FileTree fileTree)
                fileTree.RecalculateSize();
            Size += child.Size;
        }
    }
}