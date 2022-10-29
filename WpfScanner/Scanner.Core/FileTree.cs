using System.Collections.Concurrent;

namespace Scanner.Core;

public class FileTree
{
    // Constructor for folders
    public FileTree(string name) => Name = name;

    // Constructor for files
    public FileTree(string name, int size) : this(name) => Size = size;
    
    public string Name { get; }
    
    public int Size { get; private set; }

    public IList<FileTree>? Children { get; set; }

    public void RecalculateSize()
    {
        if(Children == null)
            return;
        
        Size = 0;
        foreach (var child in Children)
        {
            child.RecalculateSize();
            Size += child.Size;
        }
    }
}