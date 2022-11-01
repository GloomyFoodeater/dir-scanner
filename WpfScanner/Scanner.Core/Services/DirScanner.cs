using System.Collections.Concurrent;
using System.Diagnostics;
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
        if (!Directory.Exists(sourcePath))
            throw new ArgumentException($"Directory '{sourcePath}' did not exist");
        sourcePath = Path.GetFullPath(sourcePath); // Exceptions: ArgumentNull, Argument, PathTooLong, Security

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
            if (_queue.TryDequeue(out scanningTask))
            {
                try
                {
                    _semaphore.Wait(_tokenSource.Token);
                    scanningTask.Start();
                }
                catch
                {
                    // Exceptions: OperationCancelled, InvalidOperation 
                    break;
                }
            }

        root.RecalculateSize();
        return root;
    }

    public void Cancel() => _tokenSource?.Cancel(); // Cancel via token

    private void ScanningTaskCallback(object? param)
    {
        var root = (FileTree)param!;
        try
        {
            // Get files.
            DirectoryInfo rootInfo = new(root.Path); // Exceptions: Security, Argument, ArgumentNull, PathTooLong
            var fileSystemInfos = rootInfo.GetFileSystemInfos(); // Exceptions: DirectoryNotFound

            // Iterate over files.
            foreach (var info in fileSystemInfos)
                ProcessFile(root, info); // Exceptions: OperationCanceled
        }
        catch (OperationCanceledException)
        {
            // Task was cancelled via cancellation token.
            throw;
        }
        catch (Exception e)
        {
            // Exceptions: Security, Argument, ArgumentNull, DirectoryNotFound, PathTooLong, FileNotFound, IO
            Debug.WriteLine(e);
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
        try
        {
            var name = info.FullName; // Exceptions: Security, PathTooLong
            switch (info)
            {
                case FileInfo { LinkTarget: null } fileInfo:
                {
                    // Append file nodes with their sizes.
                    var size = fileInfo.Length; // Exceptions: FileNotFound, IO
                    FileNode fileNode = new(name, size);
                    children.Add(fileNode);
                    break;
                }
                case DirectoryInfo { LinkTarget: null }:
                {
                    // Add new directory node.
                    FileTree dirNode = new(name, new ConcurrentBag<FileNode>());
                    children.Add(dirNode);

                    // Enqueue task.
                    Task scanningTask = new(ScanningTaskCallback, dirNode, token);
                    _queue!.Enqueue(scanningTask);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            // Exceptions: FileNotFound, IO, Security, PathTooLong
            Debug.WriteLine(e);
        }
    }
}