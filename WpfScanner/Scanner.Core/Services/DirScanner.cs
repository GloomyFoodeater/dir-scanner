using System.Collections.Concurrent;
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
        // Validate source
        if (!Directory.Exists(source))
            throw new ArgumentException($"Given directory '{source}' did not exist");

        // Init data for multithreading
        _tokenSource = new CancellationTokenSource();
        _semaphore = new SemaphoreSlim(_maxRunningThreads, _maxRunningThreads);
        _queue = new ConcurrentQueue<Task>();

        // Enqueue task for root
        FileTree root = new (Path.GetFullPath(source), new List<FileNode>());
        Task? scanningTask = new (ScanningTaskCallback, root, _tokenSource.Token);
        _queue.Enqueue(scanningTask);

        // Loop over queue
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
        // Cancel via token
        _tokenSource?.Cancel();
        _tokenSource?.Dispose();
        _tokenSource = null;
    }

    private void ScanningTaskCallback(object? param)
    {
        var root = (FileTree)param!;
        CancellationToken token = _tokenSource!.Token;
        
        // Get files and directories
        DirectoryInfo rootInfo = new(root.Name);
        FileSystemInfo[] fileSystemInfos;
        try
        {
            fileSystemInfos = rootInfo.GetFileSystemInfos();
        }
        catch (DirectoryNotFoundException)
        {
            // Root directory did not exist
            _semaphore!.Release();
            return;
        }

        // Iterate over files
        foreach (var info in fileSystemInfos) 
            HandleFile(root, info);

        _semaphore!.Release();
    }

    private void HandleFile(FileTree root, FileSystemInfo info)
    {
        // TODO: Check access & existence of files 
        
        CancellationToken token = _tokenSource!.Token;
        
        token.ThrowIfCancellationRequested(); // Cancel
        if (info is FileInfo fileInfo)
        {
            // Append file nodes with their sizes
            if (fileInfo.LinkTarget == null)
            {
                FileNode fileNode = new(fileInfo.FullName, fileInfo.Length);
                root.Children.Add(fileNode);
            }
        }
        else if (info is DirectoryInfo dirInfo)
        {
            // Add new directory node
            FileTree dirNode = new(dirInfo.FullName, new List<FileNode>());
            root.Children.Add(dirNode);

            // Enqueue task
            Task scanningTask = new(ScanningTaskCallback, dirNode, token);
            _queue!.Enqueue(scanningTask);
        }
    }
}