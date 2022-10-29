using Scanner.Core.Models;

namespace Scanner.Core.Interfaces;

public interface IDirScanner
{
    FileTree Scan(string sourcePath);

    void Cancel();
}