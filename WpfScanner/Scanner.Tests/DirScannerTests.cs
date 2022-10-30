using Scanner.Core.Services;
using Scanner.Tests.DirMakers;
using Scanner.Tests.Static;

namespace Scanner.Tests;

public class DirScannerTests
{
    private const int ThreadCount = 5;

    [Fact]
    public void ParamsValidation()
    {
        // TODO: Implement this.
        throw new NotImplementedException();
    }

    [Fact]
    public void EmptyDirectory()
    {
        // Arrange
        const string path = "test-empty";
        var dirScanner = new DirScanner(ThreadCount);
        var dirMaker = new EmptyDirMaker();

        // Act
        var expectedTree = dirMaker.Create(path);
        var actualTree = dirScanner.Scan(path);

        // Assert
        Assert.NotNull(actualTree);
        Assert.True(actualTree.IsEqualTo(expectedTree));
    }

    [Fact]
    public void OneFileDirectory()
    {
        // TODO: Implement this.
        throw new NotImplementedException();
    }

    [Fact]
    public void ManyFilesDirectory()
    {
        // TODO: Implement this.
        throw new NotImplementedException();
    }

    [Fact]
    public void MultiLayeredDirectory()
    {
        // TODO: Implement this.
        throw new NotImplementedException();
    }

    [Fact]
    public void DirectoryWithLinks()
    {
        // TODO: Implement this.
        throw new NotImplementedException();
    }

    [Fact]
    public void ScanningCancellation()
    {
        // TODO: Implement this.
        throw new NotImplementedException();
    }
}