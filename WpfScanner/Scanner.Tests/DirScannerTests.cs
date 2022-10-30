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
        // Arrange
        var path = "test-invalid";
        var dirScanner = new DirScanner(1);
        var dirMaker = new OneFileDirMaker();
        var resultTree = dirMaker.Create(path);
        var filePath = resultTree.Children.First().Path;
        
        // Act & assert
        
        // Invalid number of threads.
        Assert.Throws<ArgumentException>(() => new DirScanner(0));
        
        // Nonexistent directory to scan.
        Assert.Throws<ArgumentException>(() => dirScanner.Scan(@$"{path}\non-existent"));
        
        // Given path exists, but was not path to directory.
        Assert.Throws<ArgumentException>(() => dirScanner.Scan(filePath));
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
        Assert.True(actualTree.IsEqualTo(expectedTree));
    }

    [Fact]
    public void OneFileDirectory()
    {
        // Arrange
        var path = "test-one-file";
        var dirScanner = new DirScanner(ThreadCount);
        var dirMaker = new OneFileDirMaker();
        
        // Act
        var expectedTree = dirMaker.Create(path);
        var actualTree = dirScanner.Scan(path);
        
        // Assert
        Assert.True(actualTree.IsEqualTo(expectedTree));
    }

    [Fact]
    public void ManyFilesDirectory()
    {
        // Arrange
        var path = "test-many-files";
        var dirScanner = new DirScanner(ThreadCount);
        var dirMaker = new ManyFilesDirMaker();
        
        // Act
        var expectedTree = dirMaker.Create(path);
        var actualTree = dirScanner.Scan(path);
        
        // Assert
        Assert.True(actualTree.IsEqualTo(expectedTree));
    }

    [Fact]
    public void MultiLayeredDirectory()
    {
        // Arrange
        var path = "test-multi-layered-files";
        var dirScanner = new DirScanner(ThreadCount);
        var dirMaker = new MultiLayeredDirMaker();
        
        // Act
        var expectedTree = dirMaker.Create(path);
        var actualTree = dirScanner.Scan(path);
        
        // Assert
        Assert.True(actualTree.IsEqualTo(expectedTree));
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