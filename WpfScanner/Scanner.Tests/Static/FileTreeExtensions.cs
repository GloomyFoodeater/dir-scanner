using Scanner.Core.Models;

namespace Scanner.Tests.Static;

internal static class FileTreeExtensions
{
    public static bool IsEqualTo(this FileNode self, FileNode other)
    {
        // Convert nodes to trees.
        var selfTree = self as FileTree;
        var otherTree = other as FileTree;

        // Check nodes relation.
        var areBothTrees = selfTree != null && otherTree != null;
        var areEqual = (areBothTrees || (selfTree == null && otherTree == null))
                       && self.Path == other.Path
                       && self.Size == other.Size;


        // Sort children by paths, zip them and check their relation.
        if (areBothTrees)
            areEqual = areEqual && selfTree!
                .Children.OrderBy(node => node.Path)
                .Zip(otherTree!.Children.OrderBy(node => node.Path))
                .All(pair => pair.First.IsEqualTo(pair.Second));

        return areEqual;
    }

    public static bool IsLesserThan(this FileNode self, FileNode other)
    {
        // Convert nodes to trees.
        var selfTree = self as FileTree;
        var otherTree = other as FileTree;

        // Check nodes relation.
        var areBothTrees = selfTree != null && otherTree != null;
        var isSubset = (areBothTrees || (selfTree == null && otherTree == null))
                       && self.Path == other.Path
                       && self.Size <= other.Size;

        if (areBothTrees)
        {
            try
            {
                // Check if all children nodes in self are in relation with some child node of other.  
                isSubset = isSubset && selfTree!.Children
                    .All(selfChild => selfChild
                        .IsLesserThan(otherTree!.Children
                            .First(otherChild => selfChild.Path == otherChild.Path &&
                                                 (selfChild is FileTree && otherChild is FileTree ||
                                                  selfChild is not FileTree && otherChild is not FileTree))));
            }
            catch (InvalidOperationException)
            {
                // First() did not found suitable otherChild.
                isSubset = false;
            }
        }

        return isSubset;
    }
}