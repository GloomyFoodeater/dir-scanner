using System.Collections.Concurrent;
using System.Security;
using Scanner.Core.Interfaces;
using Scanner.Core.Models;

namespace Scanner.Core.Services;

public class DirScanner : IDirScanner
{
    private readonly int _maxRunningThreads;
    private CancellationTokenSource? _tokenSource;
    private SemaphoreSlim? _semaphore;
    private ConcurrentQueue<Task>? _queue;

    public DirScanner(int maxRunningThreads)
    {
        if (maxRunningThreads < 1)
            throw new ArgumentException($"Given number of threads ({maxRunningThreads}) can not be lesser than 1");
        _maxRunningThreads = maxRunningThreads;
    }

    public FileTree Scan(string sourcePath)
    {
        // Validate source & get full path.
        try
        {
            sourcePath = Path.GetFullPath(sourcePath);
        }
        catch (SecurityException)
        {
            throw new SecurityException($"Given path {sourcePath} was not accessible");
        }
        catch
        {
            // Exceptions: ArgumentNull, Argument, PathTooLong
            throw new ArgumentException($"Failed to restore full path from {sourcePath}");
        }

        // Init data for multithreading.
        _tokenSource = new();
        _semaphore = new(_maxRunningThreads, _maxRunningThreads);
        _queue = new();

        // Enqueue task for root.
        FileTree root = new(sourcePath, new ConcurrentBag<FileNode>());
        Task? scanningTask = new(ScanningTaskCallback, root, _tokenSource.Token);
        _queue.Enqueue(scanningTask);

        // Loop over queue.
        while (_semaphore.CurrentCount != _maxRunningThreads || !_queue.IsEmpty)
        {
            _queue.TryDequeue(out scanningTask);
            if (scanningTask != null)
            {
                _semaphore.Wait();
                scanningTask.Start();
            }
        }

        return root;
    }

    public void Cancel()
    {
        // Cancel via token.
        _tokenSource?.Cancel();
    }

    private void ScanningTaskCallback(object? param)
    {
        var root = (FileTree)param!;
        try
        {
            // Get files.
            DirectoryInfo rootInfo = new(root.Path); // Exceptions: Security, Argument, ArgumentNull, PathTooLong
            var fileSystemInfos = rootInfo.GetFileSystemInfos(); // Exceptions: DirectoryNotFoundException

            // Iterate over files.
            foreach (var info in fileSystemInfos)
                ProcessFile(root, info);
        }
        catch (OperationCanceledException)
        {
            // Task was cancelled via cancellation token.
            throw;
        }
        catch
        {
            // Exceptions: Security, Argument, ArgumentNull, DirectoryNotFound, PathTooLong, FileNotFound, IO
            // Ignore.
        }
        finally
        {
            _semaphore!.Release();
        }
    }

    private void ProcessFile(FileTree root, FileSystemInfo info)
    {
        var token = _tokenSource!.Token;
        token.ThrowIfCancellationRequested(); // Cancel

        var children = (root.Children as ConcurrentBag<FileNode>)!;

        string name = info.FullName; // Exceptions: Security, PathTooLong
        if (info is FileInfo fileInfo && fileInfo.LinkTarget == null)
        {
            // Append file nodes with their sizes.
            var size = fileInfo.Length; // Exceptions: FileNotFound, IO
            FileNode fileNode = new(name, size);
            children.Add(fileNode);
        }
        else if (info is DirectoryInfo)
        {
            // Add new directory node.
            FileTree dirNode = new(name, new ConcurrentBag<FileNode>());
            children.Add(dirNode);

            // Enqueue task.
            Task scanningTask = new(ScanningTaskCallback, dirNode, token);
            _queue!.Enqueue(scanningTask);
        }
    }
}