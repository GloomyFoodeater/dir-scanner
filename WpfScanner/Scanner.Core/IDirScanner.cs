using Scanner.Core;

namespace Scanner.Core;

public interface IDirScanner
{
    FileTree Start(string source);
    
    void Cancel();
}