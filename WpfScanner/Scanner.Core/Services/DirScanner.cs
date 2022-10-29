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

    public FileTree Start(string source)
    {
        // Validate source & get full path.
        try
        {
            source = Path.GetFullPath(source);
        }
        catch (SecurityException)
        {
            throw new SecurityException($"Given path {source} was not accessible");
        }
        catch
        {
            // Exceptions: ArgumentNull, Argument, PathTooLong exceptions
            throw new ArgumentException($"Failed to restore full path from {source}");
        }

        // Init data for multithreading.
        _tokenSource = new();
        _semaphore = new(_maxRunningThreads, _maxRunningThreads);
        _queue = new();

        // Enqueue task for root.
        FileTree root = new(source, new List<FileNode>());
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
            DirectoryInfo rootInfo = new(root.Name); // Exceptions: Security, Argument, ArgumentNull, PathTooLong
            var fileSystemInfos = rootInfo.GetFileSystemInfos(); // Exceptions: DirectoryNotFoundException

            // Iterate over files.
            foreach (var info in fileSystemInfos)
                HandleFile(root, info);
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

    private void HandleFile(FileTree root, FileSystemInfo info)
    {
        CancellationToken token = _tokenSource!.Token;
        token.ThrowIfCancellationRequested(); // Cancel

        string name = info.FullName; // Exceptions: Security, PathTooLong
        if (info is FileInfo fileInfo)
        {
            // Append file nodes with their sizes.
            if (fileInfo.LinkTarget == null)
            {
                long size = fileInfo.Length; // Exceptions: FileNotFound, IO
                FileNode fileNode = new(name, size);
                root.Children.Add(fileNode);
            }
        }
        else if (info is DirectoryInfo)
        {
            // Add new directory node.
            FileTree dirNode = new(name, new List<FileNode>());
            root.Children.Add(dirNode);

            // Enqueue task.
            Task scanningTask = new(ScanningTaskCallback, dirNode, token);
            _queue!.Enqueue(scanningTask);
        }
    }
}