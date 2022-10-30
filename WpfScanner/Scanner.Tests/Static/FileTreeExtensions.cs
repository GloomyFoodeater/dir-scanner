using Scanner.Core.Models;

namespace Scanner.Tests.Static;

internal static class FileTreeExtensions
{
    public static bool IsEqualTo(this FileNode self, FileNode other)
    {
        // Convert nodes to trees.
        var selfTree = self as FileTree;
        var otherTree = other as FileTree;

        bool areBothTrees = selfTree != null && otherTree != null;
        bool areEqual = (areBothTrees || selfTree == null && otherTree == null) && 
                        self.Path == other.Path && 
                        self.Size == other.Size;
        
        // Both nodes are not leafs.
        if (areBothTrees)
            foreach (var (selfChild, otherChild) in selfTree!.Children.Zip(otherTree!.Children))
                areEqual &= selfChild.IsEqualTo(otherChild);

        return areEqual;
    }

    public static bool IsSubsetOf(this FileNode self, FileNode other)
    {
        // TODO: Implement this.
        throw new NotImplementedException();
    }
}