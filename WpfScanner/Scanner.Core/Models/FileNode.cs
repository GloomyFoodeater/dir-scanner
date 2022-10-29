namespace Scanner.Core.Models;

public class FileNode
{
    public FileNode(string path, long size) : this(path) => Size = size;

    protected FileNode(string path) => Path = path;

    public string Path { get; }

    public long Size { get; protected set; }
}