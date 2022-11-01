using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Scanner.Core.Interfaces;
using Scanner.Core.Services;
using WpfApp.Commands;
using WpfApp.Model;

namespace WpfApp.ViewModel;

public sealed class AppViewModel : INotifyPropertyChanged
{
    private const int MaxThreadCount = 100;
    private readonly IDirScanner _scanner = new DirScanner(MaxThreadCount);

    private ObservableCollection<TreeModel>? _currentTree;

    private bool _isScanning;
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
                        IsScanning = true;
                        try
                        {
                            var result = _scanner.Scan(_selectedPath!);

                            // Wrap result with collection and tree model. 
                            CurrentTree = new() { new(result) };
                            SelectedPath = Path.GetFullPath(_selectedPath!);
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK);
                        }

                        IsScanning = false;
                    }
                );
            },
            canExecute: _ => SelectedPath != null && !IsScanning);

        CancelCommand = new(
            execute: _ =>
            {
                _scanner.Cancel();
                IsScanning = false;
            });
    }

    public ObservableCollection<TreeModel>? CurrentTree
    {
        get => _currentTree;
        private set
        {
            _currentTree = value;
            OnPropertyChanged();
        }
    }

    public bool IsScanning
    {
        get => _isScanning;
        set
        {
            _isScanning = value;
            OnPropertyChanged();
        }
    }

    public string? SelectedPath
    {
        get => _selectedPath;
        set
        {
            _selectedPath = value;
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