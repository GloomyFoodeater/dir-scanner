namespace Scanner.Core.Models;

public class FileNode
{
    public FileNode(string name, long size)
    {
        Name = name;
        Size = size;
    }

    public string Name { get; }
    
    public long Size { get; protected set; }
}