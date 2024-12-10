using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public interface IReadOnlyTreeNode<T>
    {
        T Value { get; }
        IEnumerable<IReadOnlyTreeNode<T>> Nodes { get; }
        bool Contains(T value);
        bool TryFind(T value, out IReadOnlyTreeNode<T> node);
        bool SubtreeContains(T value);
        ITreeNode<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null);
        IEnumerable<IReadOnlyTreeNode<T>> AllNodes_ParentsFirst { get; }
        IEnumerable<IReadOnlyTreeNode<T>> AllNodes_ChildrenFirst { get; }
        IEnumerable<IReadOnlyTreeNode<T>> AllNodes_FromTheTop { get; }
        IEnumerable<IReadOnlyTreeNode<T>> AllNodes_FromTheBottom { get; }
    }

    public interface ITreeNode<T>
    {
        T Value { get; }
        IEnumerable<ITreeNode<T>> Nodes { get; }
        ITreeNode<T> Add(T value);
        ITreeNode<T> AddTo(T value, T parent);
        void AddRange(IEnumerable<T> values);
        void AddRangeTo(IEnumerable<T> values, T parent);
        void Clear();
        bool Contains(T value);
        bool TryFind(T value, out ITreeNode<T> node);
        bool SubtreeContains(T value);
        void Remove(T value);
        void Sort(Comparison<T> comparison);
        void SortSubtree(Comparison<T> comparison);
        ITreeNode<Target> Translate<Target>(Func<T, Target> selector, Func<T, bool> guard = null);
        IEnumerable<ITreeNode<T>> AllNodes_ParentsFirst { get; }
        IEnumerable<ITreeNode<T>> AllNodes_ChildrenFirst { get; }
        IEnumerable<ITreeNode<T>> AllNodes_FromTheTop { get; }
        IEnumerable<ITreeNode<T>> AllNodes_FromTheBottom { get; }
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

        [JsonIgnore]
        public TreeNode<T> Parent { get; private set; }

        private readonly List<TreeNode<T>> NodesList = new List<TreeNode<T>>();

        public IEnumerable<TreeNode<T>> Nodes
            => NodesList;

        [JsonIgnore]
        public IEnumerable<TreeNode<T>> AllNodes
            => AllNodes_ParentsFirst;

        [JsonIgnore]
        public IEnumerable<TreeNode<T>> AllNodes_ParentsFirst
            => NodesList
                .SelectMany(item => item.AllNodes_ParentsFirst)
                .Prepend(this);

        [JsonIgnore]
        public IEnumerable<TreeNode<T>> AllNodes_ChildrenFirst
            => NodesList
                .SelectMany(item => item.AllNodes_ChildrenFirst)
                .Append(this);

        [JsonIgnore]
        public IEnumerable<TreeNode<T>> AllNodes_FromTheTop
            => GetAllNodesLeveled(0)
                .OrderByDescending(item => item.Level)
                .Select(item => item.Node);

        [JsonIgnore]
        public IEnumerable<TreeNode<T>> AllNodes_FromTheBottom
            => GetAllNodesLeveled(0)
                .OrderBy(item => item.Level)
                .Select(item => item.Node);

        private IEnumerable<(TreeNode<T> Node, int Level)> GetAllNodesLeveled(int level)
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
            => AllNodes.FirstOrDefault(item => item.Value.Equals(parent))?.Add(value);

        public void AddRangeTo(IEnumerable<T> values, T parent)
            => AllNodes.FirstOrDefault(item => item.Value.Equals(parent))?.AddRange(values);

        public void Clear()
            => NodesList.Clear();

        public bool Contains(T value)
            => NodesList.Exists(t => t.Value.Equals(value));

        public bool SubtreeContains(T value)
            => AllNodes.ToList().Exists(t => t.Value.Equals(value));

        public bool TryFind(T value, out TreeNode<T> node)
            => (node = AllNodes.Where(node => node != this).FirstOrDefault(node => node.Value.Equals(value))) != null;

        public void Remove(T value)
            => NodesList.RemoveAll(t => t.Value.Equals(value));

        public void SubtreeRemove(T value)
        {
            AssignParents();
            AllNodes.FirstOrDefault(node => node.Value.Equals(value))?.Parent.Remove(value);
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


        #region ITree
        T ITreeNode<T>.Value
            => Value;
        IEnumerable<ITreeNode<T>> ITreeNode<T>.Nodes
            => Nodes;
        ITreeNode<T> ITreeNode<T>.Add(T value)
            => Add(value);
        ITreeNode<T> ITreeNode<T>.AddTo(T value, T parent)
            => AddTo(value, parent);
        void ITreeNode<T>.Clear()
            => Clear();
        bool ITreeNode<T>.Contains(T value)
            => Contains(value);
        void ITreeNode<T>.Remove(T value)
            => Remove(value);
        ITreeNode<Target> ITreeNode<T>.Translate<Target>(Func<T, Target> selector, Func<T, bool> guard)
            => Translate(selector, guard);
        IEnumerable<ITreeNode<T>> ITreeNode<T>.AllNodes_ParentsFirst
            => AllNodes_ParentsFirst;
        IEnumerable<ITreeNode<T>> ITreeNode<T>.AllNodes_ChildrenFirst
            => AllNodes_ChildrenFirst;
        IEnumerable<ITreeNode<T>> ITreeNode<T>.AllNodes_FromTheTop
            => AllNodes_FromTheTop;
        IEnumerable<ITreeNode<T>> ITreeNode<T>.AllNodes_FromTheBottom
            => AllNodes_FromTheBottom;
        #endregion

        #region IReadOnlyTree
        T IReadOnlyTreeNode<T>.Value
            => Value;
        IEnumerable<IReadOnlyTreeNode<T>> IReadOnlyTreeNode<T>.Nodes
            => Nodes;
        bool IReadOnlyTreeNode<T>.Contains(T value)
            => Contains(value);
        ITreeNode<Target> IReadOnlyTreeNode<T>.Translate<Target>(Func<T, Target> selector, Func<T, bool> guard)
            => Translate(selector, guard);

        bool ITreeNode<T>.TryFind(T value, out ITreeNode<T> node)
            => (TryFind(value, out var nodeObj)
                ? node = nodeObj
                : node = null) != null;

        bool IReadOnlyTreeNode<T>.TryFind(T value, out IReadOnlyTreeNode<T> node)
            => (TryFind(value, out var nodeObj)
                ? node = nodeObj
                : node = null) != null;

        IEnumerable<IReadOnlyTreeNode<T>> IReadOnlyTreeNode<T>.AllNodes_ParentsFirst
            => AllNodes_ParentsFirst;
        IEnumerable<IReadOnlyTreeNode<T>> IReadOnlyTreeNode<T>.AllNodes_ChildrenFirst
            => AllNodes_ChildrenFirst;
        IEnumerable<IReadOnlyTreeNode<T>> IReadOnlyTreeNode<T>.AllNodes_FromTheTop
            => AllNodes_FromTheTop;
        IEnumerable<IReadOnlyTreeNode<T>> IReadOnlyTreeNode<T>.AllNodes_FromTheBottom
            => AllNodes_FromTheBottom;
        #endregion
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
        IEnumerable<ITreeNode<T>> AllNodes { get; }
        IEnumerable<ITreeNode<T>> AllNodes_ParentsFirst { get; }
        IEnumerable<ITreeNode<T>> AllNodes_ChildrenFirst { get; }
        IEnumerable<ITreeNode<T>> AllNodes_FromTheTop { get; }
        IEnumerable<ITreeNode<T>> AllNodes_FromTheBottom { get; }
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
        IEnumerable<IReadOnlyTreeNode<T>> AllNodes { get; }
        IEnumerable<IReadOnlyTreeNode<T>> AllNodes_ParentsFirst { get; }
        IEnumerable<IReadOnlyTreeNode<T>> AllNodes_ChildrenFirst { get; }
        IEnumerable<IReadOnlyTreeNode<T>> AllNodes_FromTheTop { get; }
        IEnumerable<IReadOnlyTreeNode<T>> AllNodes_FromTheBottom { get; }
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

        public bool HasValue
            => Root != null;

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
        public IEnumerable<TreeNode<T>> AllNodes
            => Root?.AllNodes ?? new TreeNode<T>[0];
        public IEnumerable<TreeNode<T>> AllNodes_ParentsFirst
            => Root?.AllNodes_ParentsFirst ?? new TreeNode<T>[0];
        public IEnumerable<TreeNode<T>> AllNodes_ChildrenFirst
            => Root?.AllNodes_ChildrenFirst ?? new TreeNode<T>[0];
        public IEnumerable<TreeNode<T>> AllNodes_FromTheTop
            => Root?.AllNodes_FromTheTop ?? new TreeNode<T>[0];
        public IEnumerable<TreeNode<T>> AllNodes_FromTheBottom
            => Root?.AllNodes_FromTheBottom ?? new TreeNode<T>[0];

        IEnumerable<ITreeNode<T>> ITree<T>.AllNodes
            => AllNodes;

        IEnumerable<ITreeNode<T>> ITree<T>.AllNodes_ParentsFirst
            => AllNodes_ParentsFirst;

        IEnumerable<ITreeNode<T>> ITree<T>.AllNodes_ChildrenFirst
            => AllNodes_ChildrenFirst;

        IEnumerable<ITreeNode<T>> ITree<T>.AllNodes_FromTheTop
            => AllNodes_FromTheTop;

        IEnumerable<ITreeNode<T>> ITree<T>.AllNodes_FromTheBottom
            => AllNodes_FromTheBottom;

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

        IEnumerable<IReadOnlyTreeNode<T>> IReadOnlyTree<T>.AllNodes
            => AllNodes;

        IEnumerable<IReadOnlyTreeNode<T>> IReadOnlyTree<T>.AllNodes_ParentsFirst
            => AllNodes_ParentsFirst;

        IEnumerable<IReadOnlyTreeNode<T>> IReadOnlyTree<T>.AllNodes_ChildrenFirst
            => AllNodes_ChildrenFirst;

        IEnumerable<IReadOnlyTreeNode<T>> IReadOnlyTree<T>.AllNodes_FromTheTop
            => AllNodes_FromTheTop;

        IEnumerable<IReadOnlyTreeNode<T>> IReadOnlyTree<T>.AllNodes_FromTheBottom
            => AllNodes_FromTheBottom;

        ITree<Target> IReadOnlyTree<T>.Translate<Target>(Func<T, Target> selector, Func<T, bool> guard)
            => Translate(selector, guard);

    }
}
