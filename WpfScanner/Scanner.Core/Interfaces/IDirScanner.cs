using Scanner.Core.Models;

namespace Scanner.Core.Interfaces;

public interface IDirScanner
{
    FileTree Start(string source);
    
    void Cancel();
}