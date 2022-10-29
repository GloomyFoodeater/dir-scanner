using Scanner.Core.Models;

namespace Scanner.Tests.Interfaces;

internal interface IDirMaker
{
    FileTree Create(string destinationPath);
}