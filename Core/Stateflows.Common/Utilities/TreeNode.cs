using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    /// <summary>Traverses trees and subtrees in depth-first or level-ordered sequences.</summary>
    /// <remarks>All node traversals include the starting node. Tree traversals return an empty sequence for a missing root.
    /// Interface overloads expect the built-in <see cref="Tree{T}"/> and <see cref="TreeNode{T}"/> implementations.
    /// Despite their names, FromTheTop visits deepest levels first and FromTheBottom visits the root level first.</remarks>
    public static class TreeExtensions
    {
        /// <summary>Enumerates a subtree depth-first, visiting each parent before its children.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The subtree root to include.</param>
        /// <returns>The root and descendants in parent-first depth-first order.</returns>
        public static IEnumerable<TreeNode<T>> GetAllNodes<T>(this TreeNode<T> treeNode)
            => treeNode.GetAllNodes_ParentsFirst();
        
        /// <summary>Enumerates a subtree in preorder, visiting parents before descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The subtree root to include.</param>
        /// <returns>The root followed by each child subtree in child-list order.</returns>
        public static IEnumerable<TreeNode<T>> GetAllNodes_ParentsFirst<T>(this TreeNode<T> treeNode)
            => treeNode.NodesList
                .SelectMany(item => item.GetAllNodes_ParentsFirst())
                .Prepend(treeNode);
        
        /// <summary>Enumerates a subtree in postorder, visiting descendants before their parent.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The subtree root to include.</param>
        /// <returns>Each child subtree in child-list order followed by the root.</returns>
        public static IEnumerable<TreeNode<T>> GetAllNodes_ChildrenFirst<T>(this TreeNode<T> treeNode)
            => treeNode.NodesList
                .SelectMany(item => item.GetAllNodes_ChildrenFirst())
                .Append(treeNode);
        
        /// <summary>Enumerates nodes by descending depth, with the subtree root last.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The subtree root to include.</param>
        /// <returns>The deepest level first and progressively shallower levels afterward.</returns>
        public static IEnumerable<TreeNode<T>> GetAllNodes_FromTheTop<T>(this TreeNode<T> treeNode)
            => treeNode.GetAllNodesLeveled(0)
                .OrderByDescending(item => item.Level)
                .Select(item => item.Node);
        
        /// <summary>Enumerates nodes by ascending depth, starting at the subtree root.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The subtree root to include.</param>
        /// <returns>The root level first and progressively deeper levels afterward.</returns>
        public static IEnumerable<TreeNode<T>> GetAllNodes_FromTheBottom<T>(this TreeNode<T> treeNode)
            => treeNode.GetAllNodesLeveled(0)
                .OrderBy(item => item.Level)
                .Select(item => item.Node);
        
        /// <summary>Enumerates a mutable subtree in parent-first depth-first order.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The built-in node exposed through its mutable interface.</param>
        /// <returns>The starting node and descendants in preorder.</returns>
        public static IEnumerable<ITreeNode<T>> GetAllNodes<T>(this ITreeNode<T> treeNode)
            => treeNode.GetAllNodes_ParentsFirst();
        /// <summary>Enumerates a mutable subtree with each parent before its descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The built-in subtree root.</param>
        /// <returns>The nodes in depth-first preorder.</returns>
        public static IEnumerable<ITreeNode<T>> GetAllNodes_ParentsFirst<T>(this ITreeNode<T> treeNode)
            => GetAllNodes_ParentsFirst(treeNode as TreeNode<T>);
        /// <summary>Enumerates a mutable subtree with each parent after its descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The built-in subtree root.</param>
        /// <returns>The nodes in depth-first postorder.</returns>
        public static IEnumerable<ITreeNode<T>> GetAllNodes_ChildrenFirst<T>(this ITreeNode<T> treeNode)
            => GetAllNodes_ChildrenFirst(treeNode as TreeNode<T>);
        /// <summary>Enumerates a mutable subtree from the deepest level to its root level.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The built-in subtree root.</param>
        /// <returns>The nodes ordered by descending depth.</returns>
        public static IEnumerable<ITreeNode<T>> GetAllNodes_FromTheTop<T>(this ITreeNode<T> treeNode)
            => GetAllNodes_FromTheTop(treeNode as TreeNode<T>);
        /// <summary>Enumerates a mutable subtree from its root level to the deepest level.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The built-in subtree root.</param>
        /// <returns>The nodes ordered by ascending depth.</returns>
        public static IEnumerable<ITreeNode<T>> GetAllNodes_FromTheBottom<T>(this ITreeNode<T> treeNode)
            => GetAllNodes_FromTheBottom(treeNode as TreeNode<T>);
        
        /// <summary>Enumerates a read-only subtree in parent-first depth-first order.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The built-in node exposed through its read-only interface.</param>
        /// <returns>The starting node and descendants in preorder.</returns>
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes<T>(this IReadOnlyTreeNode<T> treeNode)
            => treeNode.GetAllNodes_ParentsFirst();
        /// <summary>Enumerates a read-only subtree with parents before descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The built-in subtree root.</param>
        /// <returns>The nodes in depth-first preorder.</returns>
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_ParentsFirst<T>(this IReadOnlyTreeNode<T> treeNode)
            => GetAllNodes_ParentsFirst(treeNode as TreeNode<T>);
        /// <summary>Enumerates a read-only subtree with parents after descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The built-in subtree root.</param>
        /// <returns>The nodes in depth-first postorder.</returns>
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_ChildrenFirst<T>(this IReadOnlyTreeNode<T> treeNode)
            => GetAllNodes_ChildrenFirst(treeNode as TreeNode<T>);
        /// <summary>Enumerates a read-only subtree from deepest level to root level.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The built-in subtree root.</param>
        /// <returns>The nodes ordered by descending depth.</returns>
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_FromTheTop<T>(this IReadOnlyTreeNode<T> treeNode)
            => GetAllNodes_FromTheTop(treeNode as TreeNode<T>);
        /// <summary>Enumerates a read-only subtree from root level to deepest level.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="treeNode">The built-in subtree root.</param>
        /// <returns>The nodes ordered by ascending depth.</returns>
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_FromTheBottom<T>(this IReadOnlyTreeNode<T> treeNode)
            => GetAllNodes_FromTheBottom(treeNode as TreeNode<T>);


        /// <summary>Enumerates a tree in parent-first depth-first order.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The tree to traverse.</param>
        /// <returns>The root and descendants in preorder, or an empty sequence for an empty tree.</returns>
        public static IEnumerable<TreeNode<T>> GetAllNodes<T>(this Tree<T> tree)
            => tree.Root?.GetAllNodes() ?? new TreeNode<T>[0];
        /// <summary>Enumerates a tree with each parent before its descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The tree to traverse.</param>
        /// <returns>The nodes in preorder, or an empty sequence for an empty tree.</returns>
        public static IEnumerable<TreeNode<T>> GetAllNodes_ParentsFirst<T>(this Tree<T> tree)
            => tree.Root?.GetAllNodes_ParentsFirst() ?? new TreeNode<T>[0];
        /// <summary>Enumerates a tree with each parent after its descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The tree to traverse.</param>
        /// <returns>The nodes in postorder, or an empty sequence for an empty tree.</returns>
        public static IEnumerable<TreeNode<T>> GetAllNodes_ChildrenFirst<T>(this Tree<T> tree)
            => tree.Root?.GetAllNodes_ChildrenFirst() ?? new TreeNode<T>[0];
        /// <summary>Enumerates a tree from its deepest level to its root.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The tree to traverse.</param>
        /// <returns>The nodes by descending depth, or an empty sequence for an empty tree.</returns>
        public static IEnumerable<TreeNode<T>> GetAllNodes_FromTheTop<T>(this Tree<T> tree)
            => tree.Root?.GetAllNodes_FromTheTop() ?? new TreeNode<T>[0];
        /// <summary>Enumerates a tree from its root level to its deepest level.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The tree to traverse.</param>
        /// <returns>The nodes by ascending depth, or an empty sequence for an empty tree.</returns>
        public static IEnumerable<TreeNode<T>> GetAllNodes_FromTheBottom<T>(this Tree<T> tree)
            => tree.Root?.GetAllNodes_FromTheBottom() ?? new TreeNode<T>[0];
        
        /// <summary>Enumerates a mutable tree in parent-first depth-first order.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The built-in tree exposed through its mutable interface.</param>
        /// <returns>The nodes in preorder, or an empty sequence when no root exists.</returns>
        public static IEnumerable<ITreeNode<T>> GetAllNodes<T>(this ITree<T> tree)
            => ((Tree<T>)tree).GetAllNodes();
        /// <summary>Enumerates a mutable tree with parents before descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The built-in tree to traverse.</param>
        /// <returns>The nodes in preorder, or an empty sequence when no root exists.</returns>
        public static IEnumerable<ITreeNode<T>> GetAllNodes_ParentsFirst<T>(this ITree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_ParentsFirst();
        /// <summary>Enumerates a mutable tree with parents after descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The built-in tree to traverse.</param>
        /// <returns>The nodes in postorder, or an empty sequence when no root exists.</returns>
        public static IEnumerable<ITreeNode<T>> GetAllNodes_ChildrenFirst<T>(this ITree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_ChildrenFirst();
        /// <summary>Enumerates a mutable tree from deepest level to root level.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The built-in tree to traverse.</param>
        /// <returns>The nodes by descending depth, or an empty sequence when no root exists.</returns>
        public static IEnumerable<ITreeNode<T>> GetAllNodes_FromTheTop<T>(this ITree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_FromTheTop();
        /// <summary>Enumerates a mutable tree from root level to deepest level.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The built-in tree to traverse.</param>
        /// <returns>The nodes by ascending depth, or an empty sequence when no root exists.</returns>
        public static IEnumerable<ITreeNode<T>> GetAllNodes_FromTheBottom<T>(this ITree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_FromTheBottom();
        
        /// <summary>Enumerates a read-only tree in parent-first depth-first order.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The built-in tree exposed through its read-only interface.</param>
        /// <returns>The nodes in preorder, or an empty sequence when no root exists.</returns>
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes<T>(this IReadOnlyTree<T> tree)
            => ((Tree<T>)tree).GetAllNodes();
        /// <summary>Enumerates a read-only tree with parents before descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The built-in tree to traverse.</param>
        /// <returns>The nodes in preorder, or an empty sequence when no root exists.</returns>
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_ParentsFirst<T>(this IReadOnlyTree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_ParentsFirst();
        /// <summary>Enumerates a read-only tree with parents after descendants.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The built-in tree to traverse.</param>
        /// <returns>The nodes in postorder, or an empty sequence when no root exists.</returns>
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_ChildrenFirst<T>(this IReadOnlyTree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_ChildrenFirst();
        /// <summary>Enumerates a read-only tree from deepest level to root level.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The built-in tree to traverse.</param>
        /// <returns>The nodes by descending depth, or an empty sequence when no root exists.</returns>
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_FromTheTop<T>(this IReadOnlyTree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_FromTheTop();
        /// <summary>Enumerates a read-only tree from root level to deepest level.</summary>
        /// <typeparam name="T">The node value type.</typeparam>
        /// <param name="tree">The built-in tree to traverse.</param>
        /// <returns>The nodes by ascending depth, or an empty sequence when no root exists.</returns>
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_FromTheBottom<T>(this IReadOnlyTree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_FromTheBottom();
    }
    
    /// <summary>A read-only view of a node's value and descendant hierarchy.</summary>
    /// <typeparam name="T">The type of value stored in each node.</typeparam>
    /// <remarks>A read-only view is not a snapshot: mutations made through another reference remain visible.</remarks>
    public interface IReadOnlyTreeNode<T>
    {
        /// <summary>Gets the value stored at this node.</summary>
        T Value { get; }
        /// <summary>Gets the immediate child nodes in their stored order.</summary>
        IEnumerable<IReadOnlyTreeNode<T>> Nodes => ((TreeNode<T>)this).Nodes;
        /// <summary>Tests the immediate children for a value, excluding this node and deeper descendants.</summary>
        /// <param name="value">The value to locate among direct children.</param>
        /// <returns>Whether an immediate child contains the value.</returns>
        bool Contains(T value);

        /// <summary>Searches descendants in parent-first depth-first order, excluding this node itself.</summary>
        /// <param name="value">The value to find.</param>
        /// <param name="node">Receives the first matching descendant, or <see langword="null"/>.</param>
        /// <returns>Whether a matching descendant was found.</returns>
        bool TryFind(T value, out IReadOnlyTreeNode<T> node)
        {
            if (((TreeNode<T>)this).TryFind(value, out var treeNode))
            {
                node = treeNode;
                return true;
            }

            node = null;
            return false;
        }
        /// <summary>Searches this node and all descendants for a value.</summary>
        /// <param name="value">The value to locate.</param>
        /// <returns>Whether this node or a descendant contains the value.</returns>
        bool SubtreeContains(T value);
        /// <summary>Creates a new subtree by projecting node values and optionally pruning branches.</summary>
        /// <typeparam name="Target">The projected value type.</typeparam>
        /// <param name="selector">Projects the value of each retained node.</param>
        /// <param name="guard">Optional predicate; rejecting a node removes that node and its whole branch.</param>
        /// <returns>The new subtree, or <see langword="null"/> if this node is rejected.</returns>
        ITreeNode<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null)
            => ((TreeNode<T>)this).Translate(selector, guard);
    }

    /// <summary>Provides operations for changing a node's child hierarchy.</summary>
    /// <typeparam name="T">The value type stored in the nodes.</typeparam>
    /// <remarks>Default interface implementations use the built-in <see cref="TreeNode{T}"/> implementation.</remarks>
    public interface ITreeNode<T>
    {
        /// <summary>Gets the value stored at this node.</summary>
        T Value { get; }
        /// <summary>Gets the immediate children in their stored order.</summary>
        IEnumerable<ITreeNode<T>> Nodes => ((TreeNode<T>)this).Nodes;
        /// <summary>Appends a new immediate child.</summary>
        /// <param name="value">The new child's value.</param>
        /// <returns>The newly added child node.</returns>
        ITreeNode<T> Add(T value) => ((TreeNode<T>)this).Add(value);
        /// <summary>Appends a child under the first matching node in this subtree, including this node.</summary>
        /// <param name="value">The new child's value.</param>
        /// <param name="parent">The parent value to locate in parent-first order.</param>
        /// <returns>The added node, or <see langword="null"/> when the parent is not found.</returns>
        ITreeNode<T> AddTo(T value, T parent) => ((TreeNode<T>)this).AddTo(value, parent);
        /// <summary>Appends immediate children in the order of the supplied values.</summary>
        /// <param name="values">The values for the new children.</param>
        void AddRange(IEnumerable<T> values) => ((TreeNode<T>)this).AddRange(values);
        /// <summary>Appends children under the first matching node in this subtree.</summary>
        /// <param name="values">The values for the new children.</param>
        /// <param name="parent">The parent value to locate; no change is made if absent.</param>
        void AddRangeTo(IEnumerable<T> values, T parent) => ((TreeNode<T>)this).AddRangeTo(values, parent);
        /// <summary>Removes all immediate children and their subtrees, retaining this node's value.</summary>
        void Clear() => ((TreeNode<T>)this).Clear();
        /// <summary>Tests only immediate child values, excluding this node and deeper descendants.</summary>
        /// <param name="value">The value to locate.</param>
        /// <returns>Whether a direct child has the value.</returns>
        bool Contains(T value) => ((TreeNode<T>)this).Contains(value);

        /// <summary>Finds the first matching descendant in parent-first order, excluding this node.</summary>
        /// <param name="value">The value to find.</param>
        /// <param name="node">Receives the descendant, or <see langword="null"/> when absent.</param>
        /// <returns>Whether a descendant was found.</returns>
        bool TryFind(T value, out ITreeNode<T> node)
        {
            if (((TreeNode<T>)this).TryFind(value, out var treeNode))
            {
                node = treeNode;
                return true;
            }

            node = null;
            return false;
        }

        /// <summary>Tests this node and all descendants for a value.</summary>
        /// <param name="value">The value to locate.</param>
        /// <returns>Whether a match exists anywhere in this subtree.</returns>
        bool SubtreeContains(T value);
        /// <summary>Removes all immediate children with a matching value, together with their descendants.</summary>
        /// <param name="value">The child value to remove.</param>
        void Remove(T value);
        /// <summary>Sorts the immediate children by their values.</summary>
        /// <param name="comparison">The comparison applied to child values.</param>
        void Sort(Comparison<T> comparison);
        /// <summary>Sorts every sibling collection below this node recursively.</summary>
        /// <param name="comparison">The comparison applied to node values.</param>
        void SortSubtree(Comparison<T> comparison);
        /// <summary>Creates a new projected subtree, pruning branches rejected by an optional guard.</summary>
        /// <typeparam name="Target">The projected node value type.</typeparam>
        /// <param name="selector">Projects each retained value.</param>
        /// <param name="guard">Optional predicate; a rejected node and its descendants are omitted.</param>
        /// <returns>The projected subtree, or <see langword="null"/> if this node is rejected.</returns>
        ITreeNode<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null)
            => ((TreeNode<T>)this).Translate(selector, guard);
        /// <summary>Exposes this node through its read-only interface without copying it.</summary>
        /// <returns>The read-only view, or <see langword="null"/> if this implementation does not support it.</returns>
        IReadOnlyTreeNode<T> AsReadOnly()
            => this as IReadOnlyTreeNode<T>;
    }

    /// <summary>A mutable node with an ordered list of children and a value fixed at construction.</summary>
    /// <typeparam name="T">The node value type.</typeparam>
    public class TreeNode<T> : ITreeNode<T>, IReadOnlyTreeNode<T>
    {
        /// <summary>Creates a node with a value and optional parent reference.</summary>
        /// <param name="value">The value stored in the node.</param>
        /// <param name="parent">The parent reference; construction does not add this node to the parent's children.</param>
        public TreeNode(T value, TreeNode<T> parent = null)
        {
            Value = value;
            Parent = parent;
        }

        private TreeNode(T value, List<TreeNode<T>> nodesList)
            : this(value, parent: null)
        {
            nodesList.ForEach(item => item.Parent = this);
            NodesList = nodesList;
        }

        private bool parentsAssigned = false;

        private void AssignParents()
        {
            if (!parentsAssigned)
            {
                NodesList.ForEach(item =>
                {
                    item.Parent = this;
                    item.AssignParents();
                });

                parentsAssigned = true;
            }
        }

        /// <summary>Gets the value assigned to this node.</summary>
        public T Value { get; private set; }

        
        /// <summary>Gets this node's parent reference, when assigned.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public TreeNode<T> Parent { get; private set; }

        internal readonly List<TreeNode<T>> NodesList = new List<TreeNode<T>>();

        /// <summary>Gets the immediate children in stored order.</summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<TreeNode<T>> Nodes
            => NodesList;
        
        internal IEnumerable<(TreeNode<T> Node, int Level)> GetAllNodesLeveled(int level)
            => NodesList
                .SelectMany(item => item.GetAllNodesLeveled(level + 1))
                .Append((this, level));

        /// <summary>Appends a cloned copy of an existing subtree as an immediate child.</summary>
        /// <param name="node">The subtree to clone and attach.</param>
        /// <returns>The attached clone, whose root parent reference is not assigned by this operation.</returns>
        public TreeNode<T> Attach(TreeNode<T> node)
        {
            node = node.Clone();
            NodesList.Add(node);
            return node;
        }

        /// <summary>Creates and appends an immediate child whose parent reference is this node.</summary>
        /// <param name="value">The child's value.</param>
        /// <returns>The newly added child.</returns>
        public TreeNode<T> Add(T value)
        {
            var tree = new TreeNode<T>(value, this);
            NodesList.Add(tree);
            return tree;
        }

        /// <inheritdoc cref="ITreeNode{T}.Sort"/>
        public void Sort(Comparison<T> comparison)
            => NodesList.Sort((node1, node2) => comparison(node1.Value, node2.Value));

        /// <inheritdoc cref="ITreeNode{T}.SortSubtree"/>
        public void SortSubtree(Comparison<T> comparison)
        {
            NodesList.Sort((node1, node2) => comparison(node1.Value, node2.Value));
            NodesList.ForEach(node => node.SortSubtree(comparison));
        }

        /// <inheritdoc cref="ITreeNode{T}.AddRange"/>
        public void AddRange(IEnumerable<T> values)
            => NodesList.AddRange(values.Select(value => new TreeNode<T>(value, this)));

        /// <summary>Adds a child under the first matching parent in this subtree, including this node.</summary>
        /// <param name="value">The new child's value.</param>
        /// <param name="parent">The parent value to find in preorder.</param>
        /// <returns>The new child, or <see langword="null"/> if the parent is absent.</returns>
        public TreeNode<T> AddTo(T value, T parent)
            => this.GetAllNodes().FirstOrDefault(item => item.Value.Equals(parent))?.Add(value);

        /// <inheritdoc cref="ITreeNode{T}.AddRangeTo"/>
        public void AddRangeTo(IEnumerable<T> values, T parent)
            => this.GetAllNodes().FirstOrDefault(item => item.Value.Equals(parent))?.AddRange(values);

        /// <inheritdoc cref="ITreeNode{T}.Clear"/>
        public void Clear()
            => NodesList.Clear();

        /// <inheritdoc cref="ITreeNode{T}.Contains"/>
        public bool Contains(T value)
            => NodesList.Exists(t => t.Value.Equals(value));

        /// <inheritdoc cref="ITreeNode{T}.SubtreeContains"/>
        public bool SubtreeContains(T value)
            => this.GetAllNodes().ToList().Exists(t => t.Value.Equals(value));

        /// <summary>Searches descendants in preorder, excluding this node itself.</summary>
        /// <param name="value">The value to find.</param>
        /// <param name="node">Receives the first matching descendant, or <see langword="null"/>.</param>
        /// <returns>Whether a descendant was found.</returns>
        public bool TryFind(T value, out TreeNode<T> node)
            => (node = this.GetAllNodes().Where(node => node != this).FirstOrDefault(node => node.Value.Equals(value))) != null;

        /// <inheritdoc cref="ITreeNode{T}.Remove"/>
        public void Remove(T value)
            => NodesList.RemoveAll(t => t.Value.Equals(value));

        /// <summary>Finds the first matching node in preorder and removes matching children from its parent.</summary>
        /// <param name="value">The value to remove from a descendant node.</param>
        /// <remarks>The root of this subtree must not be the matching node when it has no parent.
        /// Siblings with the same value are also removed because parent removal matches all direct children.</remarks>
        public void SubtreeRemove(T value)
        {
            AssignParents();
            this.GetAllNodes().FirstOrDefault(node => node.Value.Equals(value))?.Parent.Remove(value);
        }

        /// <summary>Creates a new subtree by projecting retained node values.</summary>
        /// <typeparam name="Target">The projected node value type.</typeparam>
        /// <param name="selector">Projects each retained node's value.</param>
        /// <param name="guard">Optional predicate; rejecting a node also prunes its descendants.</param>
        /// <returns>The projected subtree, or <see langword="null"/> if this node is rejected.</returns>
        public TreeNode<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null)
            => guard != null && !guard(Value)
                ? null
                : new TreeNode<Target>(
                    selector(Value),
                    NodesList
                        .Select(item => item.Translate(selector, guard))
                        .Where(item => item != null)
                        .ToList()
                );

        /// <summary>Copies the subtree structure while preserving its existing value references.</summary>
        /// <returns>A new root and descendant nodes with the same values.</returns>
        public TreeNode<T> Clone()
            => Translate(x => x);
    }

    /// <summary>A mutable hierarchy with an optional root and operations over the whole tree.</summary>
    /// <typeparam name="T">The type of node values.</typeparam>
    public interface ITree<T>
    {
        /// <summary>Gets or sets the root node; <see langword="null"/> represents an empty tree.</summary>
        ITreeNode<T> Root { get; set; }
        /// <summary>Gets whether the tree has a root, independently of the root's stored value.</summary>
        bool HasValue { get; }
        /// <summary>Gets the root's value, or the default value when the tree is empty.</summary>
        T Value { get; }
        /// <summary>Removes the root if it matches, otherwise removes the first matching descendant branch.</summary>
        /// <param name="value">The value whose branch is removed.</param>
        /// <remarks>Descendant removal also removes matching siblings of the first matching descendant.</remarks>
        void Remove(T value);
        /// <summary>Tests the root and all descendants for a value.</summary>
        /// <param name="value">The value to locate.</param>
        /// <returns>Whether any node has the value.</returns>
        bool Contains(T value);
        /// <summary>Finds the first matching node in parent-first depth-first order, including the root.</summary>
        /// <param name="value">The value to find.</param>
        /// <param name="node">Receives the matching node, or <see langword="null"/>.</param>
        /// <returns>Whether a match was found.</returns>
        bool TryFind(T value, out ITreeNode<T> node);
        /// <summary>Adds a child under the first matching parent, or creates the root for an empty tree and null parent.</summary>
        /// <param name="value">The new node's value.</param>
        /// <param name="parent">The parent value to find in preorder; null permits root creation when the tree is empty.</param>
        /// <returns>The added node, or <see langword="null"/> if no parent or root insertion point is available.</returns>
        ITreeNode<T> AddTo(T value, T parent);
        /// <summary>Sorts all sibling collections in the tree, leaving the root in place.</summary>
        /// <param name="comparison">The comparison applied to sibling values.</param>
        void Sort(Comparison<T> comparison);
        /// <summary>Creates a projected tree, optionally pruning rejected branches.</summary>
        /// <typeparam name="Target">The projected value type.</typeparam>
        /// <param name="selector">Projects retained node values.</param>
        /// <param name="guard">Optional predicate; rejecting a node removes its entire branch from the result.</param>
        /// <returns>A new tree, empty when the original is empty or its root is rejected.</returns>
        ITree<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null);
        /// <summary>Exposes this tree through its read-only interface without taking a snapshot.</summary>
        /// <returns>The read-only view, or <see langword="null"/> if unsupported by this implementation.</returns>
        IReadOnlyTree<T> AsReadOnly()
            => this as IReadOnlyTree<T>;
    }

    /// <summary>A read-only view of a hierarchy, such as a behavior's active nodes or current states.</summary>
    /// <typeparam name="T">The node value type.</typeparam>
    /// <remarks>The view is not an immutable snapshot. The root may be absent, and changes made through mutable references remain visible.</remarks>
    public interface IReadOnlyTree<T>
    {
        /// <summary>Gets the root node, or <see langword="null"/> when the tree is empty.</summary>
        IReadOnlyTreeNode<T> Root { get; }
        /// <summary>Gets whether a root node exists, regardless of its stored value.</summary>
        bool HasValue { get; }
        /// <summary>Gets the root value, or the default value when there is no root.</summary>
        T Value { get; }
        /// <summary>Tests the root and all descendants for a value.</summary>
        /// <param name="value">The value to locate.</param>
        /// <returns>Whether a node in the tree has the value.</returns>
        bool Contains(T value);
        /// <summary>Finds the first matching node in parent-first order, including the root.</summary>
        /// <param name="value">The value to find.</param>
        /// <param name="node">Receives the node, or <see langword="null"/> if absent.</param>
        /// <returns>Whether a matching node was found.</returns>
        bool TryFind(T value, out IReadOnlyTreeNode<T> node);
        /// <summary>Creates a separate projected tree, optionally pruning branches.</summary>
        /// <typeparam name="Target">The projected value type.</typeparam>
        /// <param name="selector">Projects retained node values.</param>
        /// <param name="guard">Optional predicate; rejecting a node prunes its whole branch.</param>
        /// <returns>The projected mutable tree, empty if the source is empty or the root is rejected.</returns>
        ITree<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null);
    }

    /// <summary>A mutable tree supporting mutable and read-only interface views of the same hierarchy.</summary>
    /// <typeparam name="T">The node value type.</typeparam>
    public sealed class Tree<T> : ITree<T>, IReadOnlyTree<T>
    {
        /// <summary>Gets or sets the root node; null represents an empty tree.</summary>
        public TreeNode<T> Root { get; set; }
        /// <inheritdoc />
        ITreeNode<T> ITree<T>.Root
        {
            get => Root;
            set => Root = value as TreeNode<T>;
        }
        /// <inheritdoc />
        IReadOnlyTreeNode<T> IReadOnlyTree<T>.Root
            => Root;

        /// <summary>Gets whether this tree has a root node.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasValue
            => Root != null;
        
        /// <summary>Gets the root's value, or the default value when the tree is empty.</summary>
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public T Value => HasValue
            ? Root.Value
            : default;

        /// <inheritdoc cref="ITree{T}.Remove"/>
        public void Remove(T value)
        {
            if (Root != null)
            {
                if (Root.Value.Equals(value))
                {
                    Root = null;
                }
                else
                {
                    Root.SubtreeRemove(value);
                }
            }
        }

        /// <inheritdoc cref="ITree{T}.Contains"/>
        public bool Contains(T value)
            => Root != null && (Root.Value.Equals(value) || Root.SubtreeContains(value));

        /// <summary>Finds the first matching node in preorder, including the root.</summary>
        /// <param name="value">The value to find.</param>
        /// <param name="node">Receives the matching node, or <see langword="null"/>.</param>
        /// <returns>Whether a matching node was found.</returns>
        public bool TryFind(T value, out TreeNode<T> node)
            => HasValue
                ? Value.Equals(value)
                    ? (node = Root) != null
                    : Root.TryFind(value, out node)
                : (node = null) != null;

        /// <summary>Appends a child under the first matching parent, or creates the root for an empty tree and null parent.</summary>
        /// <param name="value">The new node's value.</param>
        /// <param name="parent">The parent value to locate in preorder.</param>
        /// <returns>The added node, or <see langword="null"/> when no insertion point is available.</returns>
        public TreeNode<T> AddTo(T value, T parent)
        {
            if (Root == null && parent == null)
            {
                Root = new TreeNode<T>(value);
                return Root;
            }
            else
            {
                return Root?.AddTo(value, parent);
            }
        }
        /// <inheritdoc cref="ITree{T}.Sort"/>
        public void Sort(Comparison<T> comparison)
            => Root?.SortSubtree(comparison);
        
        /// <summary>Creates a new tree by projecting retained node values.</summary>
        /// <typeparam name="Target">The projected value type.</typeparam>
        /// <param name="selector">Projects retained values.</param>
        /// <param name="guard">Optional predicate; rejecting a node prunes its whole branch.</param>
        /// <returns>A separate tree, empty if the root is absent or rejected.</returns>
        public Tree<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null)
            => new Tree<Target> { Root = Root?.Translate(selector, guard) };

        /// <summary>Copies the tree structure while preserving the original value references.</summary>
        /// <returns>A tree with new nodes and the same values.</returns>
        public Tree<T> Clone()
            => Translate(x => x);

        /// <inheritdoc />
        ITreeNode<T> ITree<T>.AddTo(T value, T parent)
            => AddTo(value, parent);

        /// <inheritdoc />
        ITree<Target> ITree<T>.Translate<Target>(Func<T, Target> selector, Func<T, bool> guard)
            => Translate(selector, guard);

        /// <inheritdoc />
        bool ITree<T>.TryFind(T value, out ITreeNode<T> node)
            => (TryFind(value, out var nodeObj)
                ? node = nodeObj
                : node = null) != null;

        /// <inheritdoc />
        bool IReadOnlyTree<T>.TryFind(T value, out IReadOnlyTreeNode<T> node)
            => (TryFind(value, out var nodeObj)
                ? node = nodeObj
                : node = null) != null;

        /// <inheritdoc />
        ITree<Target> IReadOnlyTree<T>.Translate<Target>(Func<T, Target> selector, Func<T, bool> guard)
            => Translate(selector, guard);
    }
}
