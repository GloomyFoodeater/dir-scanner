using System.Collections.ObjectModel;
using System.IO;
using Scanner.Core.Models;

namespace WpfApp.Model;

public class TreeModel
{
    // Transform tree with 2 types of nodes into tree with 1 type of node.
    public TreeModel(FileNode originalNode)
    {
        Name = Path.GetFileName(originalNode.Path);
        
        // Empty name if path only contains drive.
        if (Name == "")
            Name = "/";
        Size = originalNode.Size;
        
        Children = new ObservableCollection<TreeModel>();
        if (originalNode is FileTree originalTree)
            foreach (var originalChild in originalTree.Children)
                Children.Add(new TreeModel(originalChild)
                {
                    Proportion = (double)originalChild.Size / originalNode.Size,
                    Icon = originalChild is FileTree ? IconEnum.Directory : IconEnum.File
                });
    }

    public string Name { get; }
    public long Size { get; }
    
    public ObservableCollection<TreeModel> Children { get; }

    public IconEnum Icon { get; private init; } = IconEnum.Root;

    public double Proportion { get; private init; } = 1.0;

}