using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public static class TreeExtensions
    {
        public static IEnumerable<TreeNode<T>> GetAllNodes<T>(this TreeNode<T> treeNode)
            => treeNode.GetAllNodes_ParentsFirst();
        
        public static IEnumerable<TreeNode<T>> GetAllNodes_ParentsFirst<T>(this TreeNode<T> treeNode)
            => treeNode.NodesList
                .SelectMany(item => item.GetAllNodes_ParentsFirst())
                .Prepend(treeNode);
        
        public static IEnumerable<TreeNode<T>> GetAllNodes_ChildrenFirst<T>(this TreeNode<T> treeNode)
            => treeNode.NodesList
                .SelectMany(item => item.GetAllNodes_ChildrenFirst())
                .Append(treeNode);
        
        public static IEnumerable<TreeNode<T>> GetAllNodes_FromTheTop<T>(this TreeNode<T> treeNode)
            => treeNode.GetAllNodesLeveled(0)
                .OrderByDescending(item => item.Level)
                .Select(item => item.Node);
        
        public static IEnumerable<TreeNode<T>> GetAllNodes_FromTheBottom<T>(this TreeNode<T> treeNode)
            => treeNode.GetAllNodesLeveled(0)
                .OrderBy(item => item.Level)
                .Select(item => item.Node);
        
        public static IEnumerable<ITreeNode<T>> GetAllNodes<T>(this ITreeNode<T> treeNode)
            => treeNode.GetAllNodes_ParentsFirst();
        public static IEnumerable<ITreeNode<T>> GetAllNodes_ParentsFirst<T>(this ITreeNode<T> treeNode)
            => GetAllNodes_ParentsFirst(treeNode as TreeNode<T>);
        public static IEnumerable<ITreeNode<T>> GetAllNodes_ChildrenFirst<T>(this ITreeNode<T> treeNode)
            => GetAllNodes_ChildrenFirst(treeNode as TreeNode<T>);
        public static IEnumerable<ITreeNode<T>> GetAllNodes_FromTheTop<T>(this ITreeNode<T> treeNode)
            => GetAllNodes_FromTheTop(treeNode as TreeNode<T>);
        public static IEnumerable<ITreeNode<T>> GetAllNodes_FromTheBottom<T>(this ITreeNode<T> treeNode)
            => GetAllNodes_FromTheBottom(treeNode as TreeNode<T>);
        
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes<T>(this IReadOnlyTreeNode<T> treeNode)
            => treeNode.GetAllNodes_ParentsFirst();
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_ParentsFirst<T>(this IReadOnlyTreeNode<T> treeNode)
            => GetAllNodes_ParentsFirst(treeNode as TreeNode<T>);
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_ChildrenFirst<T>(this IReadOnlyTreeNode<T> treeNode)
            => GetAllNodes_ChildrenFirst(treeNode as TreeNode<T>);
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_FromTheTop<T>(this IReadOnlyTreeNode<T> treeNode)
            => GetAllNodes_FromTheTop(treeNode as TreeNode<T>);
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_FromTheBottom<T>(this IReadOnlyTreeNode<T> treeNode)
            => GetAllNodes_FromTheBottom(treeNode as TreeNode<T>);


        public static IEnumerable<TreeNode<T>> GetAllNodes<T>(this Tree<T> tree)
            => tree.Root?.GetAllNodes() ?? new TreeNode<T>[0];
        public static IEnumerable<TreeNode<T>> GetAllNodes_ParentsFirst<T>(this Tree<T> tree)
            => tree.Root?.GetAllNodes_ParentsFirst() ?? new TreeNode<T>[0];
        public static IEnumerable<TreeNode<T>> GetAllNodes_ChildrenFirst<T>(this Tree<T> tree)
            => tree.Root?.GetAllNodes_ChildrenFirst() ?? new TreeNode<T>[0];
        public static IEnumerable<TreeNode<T>> GetAllNodes_FromTheTop<T>(this Tree<T> tree)
            => tree.Root?.GetAllNodes_FromTheTop() ?? new TreeNode<T>[0];
        public static IEnumerable<TreeNode<T>> GetAllNodes_FromTheBottom<T>(this Tree<T> tree)
            => tree.Root?.GetAllNodes_FromTheBottom() ?? new TreeNode<T>[0];
        
        public static IEnumerable<ITreeNode<T>> GetAllNodes<T>(this ITree<T> tree)
            => ((Tree<T>)tree).GetAllNodes();
        public static IEnumerable<ITreeNode<T>> GetAllNodes_ParentsFirst<T>(this ITree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_ParentsFirst();
        public static IEnumerable<ITreeNode<T>> GetAllNodes_ChildrenFirst<T>(this ITree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_ChildrenFirst();
        public static IEnumerable<ITreeNode<T>> GetAllNodes_FromTheTop<T>(this ITree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_FromTheTop();
        public static IEnumerable<ITreeNode<T>> GetAllNodes_FromTheBottom<T>(this ITree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_FromTheBottom();
        
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes<T>(this IReadOnlyTree<T> tree)
            => ((Tree<T>)tree).GetAllNodes();
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_ParentsFirst<T>(this IReadOnlyTree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_ParentsFirst();
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_ChildrenFirst<T>(this IReadOnlyTree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_ChildrenFirst();
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_FromTheTop<T>(this IReadOnlyTree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_FromTheTop();
        public static IEnumerable<IReadOnlyTreeNode<T>> GetAllNodes_FromTheBottom<T>(this IReadOnlyTree<T> tree)
            => ((Tree<T>)tree).GetAllNodes_FromTheBottom();
    }
    
    public interface IReadOnlyTreeNode<T>
    {
        T Value { get; }
        IEnumerable<IReadOnlyTreeNode<T>> Nodes => ((TreeNode<T>)this).Nodes;
        bool Contains(T value);

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
        bool SubtreeContains(T value);
        ITreeNode<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null)
            => ((TreeNode<T>)this).Translate(selector, guard);
    }

    public interface ITreeNode<T>
    {
        T Value { get; }
        IEnumerable<ITreeNode<T>> Nodes => ((TreeNode<T>)this).Nodes;
        ITreeNode<T> Add(T value) => ((TreeNode<T>)this).Add(value);
        ITreeNode<T> AddTo(T value, T parent) => ((TreeNode<T>)this).AddTo(value, parent);
        void AddRange(IEnumerable<T> values) => ((TreeNode<T>)this).AddRange(values);
        void AddRangeTo(IEnumerable<T> values, T parent) => ((TreeNode<T>)this).AddRangeTo(values, parent);
        void Clear() => ((TreeNode<T>)this).Clear();
        bool Contains(T value) => ((TreeNode<T>)this).Contains(value);

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

        bool SubtreeContains(T value);
        void Remove(T value);
        void Sort(Comparison<T> comparison);
        void SortSubtree(Comparison<T> comparison);
        ITreeNode<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null)
            => ((TreeNode<T>)this).Translate(selector, guard);
        IReadOnlyTreeNode<T> AsReadOnly()
            => this as IReadOnlyTreeNode<T>;
    }

    public class TreeNode<T> : ITreeNode<T>, IReadOnlyTreeNode<T>
    {
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

        public T Value { get; private set; }

        
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public TreeNode<T> Parent { get; private set; }

        internal readonly List<TreeNode<T>> NodesList = new List<TreeNode<T>>();

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<TreeNode<T>> Nodes
            => NodesList;
        
        internal IEnumerable<(TreeNode<T> Node, int Level)> GetAllNodesLeveled(int level)
            => NodesList
                .SelectMany(item => item.GetAllNodesLeveled(level + 1))
                .Append((this, level));

        public TreeNode<T> Add(T value)
        {
            var tree = new TreeNode<T>(value, this);
            NodesList.Add(tree);
            return tree;
        }

        public void Sort(Comparison<T> comparison)
            => NodesList.Sort((node1, node2) => comparison(node1.Value, node2.Value));

        public void SortSubtree(Comparison<T> comparison)
        {
            NodesList.Sort((node1, node2) => comparison(node1.Value, node2.Value));
            NodesList.ForEach(node => node.SortSubtree(comparison));
        }

        public void AddRange(IEnumerable<T> values)
            => NodesList.AddRange(values.Select(value => new TreeNode<T>(value, this)));

        public TreeNode<T> AddTo(T value, T parent)
            => this.GetAllNodes().FirstOrDefault(item => item.Value.Equals(parent))?.Add(value);

        public void AddRangeTo(IEnumerable<T> values, T parent)
            => this.GetAllNodes().FirstOrDefault(item => item.Value.Equals(parent))?.AddRange(values);

        public void Clear()
            => NodesList.Clear();

        public bool Contains(T value)
            => NodesList.Exists(t => t.Value.Equals(value));

        public bool SubtreeContains(T value)
            => this.GetAllNodes().ToList().Exists(t => t.Value.Equals(value));

        public bool TryFind(T value, out TreeNode<T> node)
            => (node = this.GetAllNodes().Where(node => node != this).FirstOrDefault(node => node.Value.Equals(value))) != null;

        public void Remove(T value)
            => NodesList.RemoveAll(t => t.Value.Equals(value));

        public void SubtreeRemove(T value)
        {
            AssignParents();
            this.GetAllNodes().FirstOrDefault(node => node.Value.Equals(value))?.Parent.Remove(value);
        }

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
    }

    public interface ITree<T>
    {
        ITreeNode<T> Root { get; set; }
        bool HasValue { get; }
        T Value { get; }
        void Remove(T value);
        bool Contains(T value);
        bool TryFind(T value, out ITreeNode<T> node);
        ITreeNode<T> AddTo(T value, T parent);
        void Sort(Comparison<T> comparison);
        ITree<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null);
        IReadOnlyTree<T> AsReadOnly()
            => this as IReadOnlyTree<T>;
    }

    public interface IReadOnlyTree<T>
    {
        IReadOnlyTreeNode<T> Root { get; }
        bool HasValue { get; }
        T Value { get; }
        bool Contains(T value);
        bool TryFind(T value, out IReadOnlyTreeNode<T> node);
        ITree<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null);
    }

    public sealed class Tree<T> : ITree<T>, IReadOnlyTree<T>
    {
        public TreeNode<T> Root { get; set; }
        ITreeNode<T> ITree<T>.Root
        {
            get => Root;
            set => Root = value as TreeNode<T>;
        }
        IReadOnlyTreeNode<T> IReadOnlyTree<T>.Root
            => Root;

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public bool HasValue
            => Root != null;
        
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public T Value => HasValue
            ? Root.Value
            : default;

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

        public bool Contains(T value)
            => Root != null && (Root.Value.Equals(value) || Root.SubtreeContains(value));

        public bool TryFind(T value, out TreeNode<T> node)
            => HasValue
                ? Value.Equals(value)
                    ? (node = Root) != null
                    : Root.TryFind(value, out node)
                : (node = null) != null;

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
        public void Sort(Comparison<T> comparison)
            => Root?.SortSubtree(comparison);
        
        public Tree<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null)
            => new Tree<Target> { Root = Root?.Translate(selector, guard) };

        ITreeNode<T> ITree<T>.AddTo(T value, T parent)
            => AddTo(value, parent);

        ITree<Target> ITree<T>.Translate<Target>(Func<T, Target> selector, Func<T, bool> guard)
            => Translate(selector, guard);

        bool ITree<T>.TryFind(T value, out ITreeNode<T> node)
            => (TryFind(value, out var nodeObj)
                ? node = nodeObj
                : node = null) != null;

        bool IReadOnlyTree<T>.TryFind(T value, out IReadOnlyTreeNode<T> node)
            => (TryFind(value, out var nodeObj)
                ? node = nodeObj
                : node = null) != null;

        ITree<Target> IReadOnlyTree<T>.Translate<Target>(Func<T, Target> selector, Func<T, bool> guard)
            => Translate(selector, guard);
    }
}
