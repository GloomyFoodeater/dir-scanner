﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Scanner.Core.Interfaces;
using Scanner.Core.Services;
using WpfApp.Commands;
using WpfApp.Model;
using System.Windows.Forms;

namespace WpfApp.ViewModel;

public sealed class AppViewModel : INotifyPropertyChanged
{
    private const int MaxThreadCount = 100;
    private readonly IDirScanner _scanner = new DirScanner(MaxThreadCount);

    private bool _isScanning;

    private ObservableCollection<TreeModel>? _currentTree;
    private string? _selectedPath;

    public AppViewModel()
    {
        OpenCommand = new(execute: _ =>
        {
            using var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
                SelectedPath = dialog.SelectedPath;
        });

        ScanCommand = new(
            execute: _ =>
            {
                Task.Run(() =>
                    {
                        _isScanning = true;
                        var result = _scanner.Scan(_selectedPath!);
                        _isScanning = false;

                        // Wrap result with collection and tree model. 
                        CurrentTree = new() { new(result) };
                    }
                );
            },
            canExecute: _ => _selectedPath != null && !_isScanning);

        CancelCommand = new(
            execute: _ =>
            {
                _scanner.Cancel();
                _isScanning = false;
            },
            canExecute: _ => _isScanning);
    }

    public ObservableCollection<TreeModel>? CurrentTree
    {
        get => _currentTree;
        set
        {
            _currentTree = value;
            OnPropertyChanged();
        }
    }

    public string? SelectedPath
    {
        get => _selectedPath;
        set
        {
            _selectedPath = value != null ? Path.GetFullPath(value) : value;
            OnPropertyChanged();
        }
    }

    public RelayCommand OpenCommand { get; }

    public RelayCommand ScanCommand { get; }

    public RelayCommand CancelCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}