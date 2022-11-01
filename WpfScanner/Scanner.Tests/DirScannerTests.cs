using System.Diagnostics;
using Scanner.Core.Models;
using Scanner.Core.Services;
using Scanner.Tests.DirMakers;
using Scanner.Tests.Static;
using Xunit.Abstractions;

namespace Scanner.Tests;

public class DirScannerTests
{
    private readonly ITestOutputHelper _helper;

    private const int ThreadCount = 100;

    public DirScannerTests(ITestOutputHelper helper) => _helper = helper;

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

    // Requires admin privileges.
    [Fact]
    public void DirectoryWithLinks()
    {
        // Arrange
        var path = "test-links";
        var dirScanner = new DirScanner(ThreadCount);
        var dirMaker = new LinksDirMaker();

        // Act
        var expectedTree = dirMaker.Create(path);
        var actualTree = dirScanner.Scan(path);

        // Assert
        Assert.True(actualTree.IsEqualTo(expectedTree));
    }

    [Fact]
    public void TreeResize()
    {
        // Arrange
        var tree = new FileTree("", new List<FileNode>
        {
            new("", 10),
            new("", 20),
            new("", 30),
            new FileTree("",
                new List<FileNode>
                {
                    new("", 20),
                    new("", 15),
                    new FileTree("", new List<FileNode>())
                }),
            new FileTree("",
                new List<FileNode>
                {
                    new("", 3),
                    new("", 22),
                    new FileTree("",
                        new List<FileNode>
                        {
                            new("", 3),
                            new("", 22),
                            new FileTree("",
                                new List<FileNode>
                                {
                                    new("", 3),
                                    new("", 22)
                                }),
                            new FileTree("",
                                new List<FileNode>
                                {
                                    new("", 3),
                                    new("", 22)
                                })
                        })
                })
        });

        // Act
        tree.RecalculateSize();

        // Assert
        Assert.Equal(195, tree.Size);
    }

    [Fact]
    public void ScanningCancellation()
    {
        // Arrange
        var path = "C:\\Users";
        DirScanner dirScanner = new(ThreadCount);
        Stopwatch stopwatch = new();

        // Act
        FileTree? subTree = null;
        var scanningTask = Task.Run(() =>
        {
            stopwatch.Start();
            subTree = dirScanner.Scan(path);
            stopwatch.Stop();
        });
        Thread.Sleep(500);
        dirScanner.Cancel();
        scanningTask.Wait();

        var dtSub = stopwatch.ElapsedMilliseconds;

        stopwatch.Start();
        FileTree supTree = dirScanner.Scan(path);
        stopwatch.Stop();
        var dtSup = stopwatch.ElapsedMilliseconds;

        // Assert
        Assert.NotNull(subTree);
        Assert.True(subTree!.IsLesserThan(supTree));

        // Debug output
        _helper.WriteLine($"{nameof(ScanningCancellation)} test results:");
        _helper.WriteLine($"\t<Sub tree>: {subTree!.Size}b, {dtSub}ms");
        _helper.WriteLine($"\t<Sup tree>: {supTree.Size}b, {dtSup}ms");
    }
}