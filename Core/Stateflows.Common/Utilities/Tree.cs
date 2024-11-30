using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Stateflows.Common
{
    public interface IReadOnlyTree<T>
    {
        T Value { get; }
        IEnumerable<IReadOnlyTree<T>> Items { get; }
        bool Contains(T value);
        IReadOnlyTree<Target> Translate<Target>(Func<T, Target> selector, Func<Target, bool> guard = null);
        IEnumerable<IReadOnlyTree<T>> AllItems_ParentsFirst { get; }
        IEnumerable<IReadOnlyTree<T>> AllItems_ChildrenFirst { get; }
        IEnumerable<IReadOnlyTree<T>> AllItems_FromTheTop { get; }
        IEnumerable<IReadOnlyTree<T>> AllItems_FromTheBottom { get; }
    }

    public interface ITree<T>
    {
        T Value { get; }
        IEnumerable<ITree<T>> Items { get; }
        IEnumerable<ITree<T>> AllItems { get; }
        ITree<T> Add(T value);
        void AddRange(IEnumerable<T> values);
        void Clear();
        bool Contains(T value);
        void Remove(T value);
        ITree<Target> Translate<Target>(Func<T, Target> selector, Func<Target, bool> guard = null);
        IEnumerable<ITree<T>> AllItems_ParentsFirst { get; }
        IEnumerable<ITree<T>> AllItems_ChildrenFirst { get; }
        IEnumerable<ITree<T>> AllItems_FromTheTop { get; }
        IEnumerable<ITree<T>> AllItems_FromTheBottom { get; }
    }

    public class Tree<T> : ITree<T>, IReadOnlyTree<T>
    {
        public Tree(T value, Tree<T> parent = null)
        {
            Value = value;
            Parent = parent;
        }

        private Tree(T value, List<Tree<T>> itemsList)
            : this(value, parent: null)
        {
            itemsList.ForEach(item => item.Parent = this);
            ItemsList = itemsList;
        }

        public T Value { get; private set; }

        [JsonIgnore]
        public Tree<T> Parent { get; private set; }

        private readonly List<Tree<T>> ItemsList = new List<Tree<T>>();

        public IEnumerable<Tree<T>> Items
            => ItemsList;

        [JsonIgnore]
        public IEnumerable<Tree<T>> AllItems_ParentsFirst
            => ItemsList
                .SelectMany(item => item.AllItems_ParentsFirst)
                .Prepend(this);

        [JsonIgnore]
        public IEnumerable<Tree<T>> AllItems_ChildrenFirst
            => ItemsList
                .SelectMany(item => item.AllItems_ChildrenFirst)
                .Append(this);

        [JsonIgnore]
        public IEnumerable<Tree<T>> AllItems_FromTheTop
            => GetAllItemsLeveled(0)
                .OrderByDescending(item => item.Level)
                .Select(item => item.Node);

        [JsonIgnore]
        public IEnumerable<Tree<T>> AllItems_FromTheBottom
            => GetAllItemsLeveled(0)
                .OrderBy(item => item.Level)
                .Select(item => item.Node);

        private IEnumerable<(Tree<T> Node, int Level)> GetAllItemsLeveled(int level)
            => ItemsList
                .SelectMany(item => item.GetAllItemsLeveled(level + 1))
                .Append((this, level));

        public Tree<T> Add(T value)
        {
            var tree = new Tree<T>(value, this);
            ItemsList.Add(tree);
            return tree;
        }

        public void AddRange(IEnumerable<T> values)
            => ItemsList.AddRange(values.Select(value => new Tree<T>(value)));

        public void Clear()
            => ItemsList.Clear();

        public bool Contains(T value)
            => ItemsList.Exists(t => t.Value.Equals(value));

        public void Remove(T value)
            => ItemsList.RemoveAll(t => t.Value.Equals(value));

        public Tree<Target> Translate<Target>(Func<T, Target> selector, Func<Target, bool> guard = null)
        {
            var value = selector(Value);
            return guard != null && !guard(value)
                ? null
                : new Tree<Target>(
                    value,
                    ItemsList
                        .Select(item => item.Translate(selector, guard))
                        .Where(item => item != null)
                        .ToList()
                );
        }

        #region ITree
        T ITree<T>.Value
            => Value;
        IEnumerable<ITree<T>> ITree<T>.Items
            => Items;
        ITree<T> ITree<T>.Add(T value)
            => Add(value);
        void ITree<T>.Clear()
            => Clear();
        bool ITree<T>.Contains(T value)
            => Contains(value);
        void ITree<T>.Remove(T value)
            => Remove(value);
        ITree<Target> ITree<T>.Translate<Target>(Func<T, Target> selector, Func<Target, bool> guard)
            => Translate(selector, guard);
        IEnumerable<ITree<T>> ITree<T>.AllItems_ParentsFirst
            => AllItems_ParentsFirst;
        IEnumerable<ITree<T>> ITree<T>.AllItems_ChildrenFirst
            => AllItems_ChildrenFirst;
        IEnumerable<ITree<T>> ITree<T>.AllItems_FromTheTop
            => AllItems_FromTheTop;
        IEnumerable<ITree<T>> ITree<T>.AllItems_FromTheBottom
            => AllItems_FromTheBottom;
        #endregion

        #region IReadOnlyTree
        T IReadOnlyTree<T>.Value
            => Value;
        IEnumerable<IReadOnlyTree<T>> IReadOnlyTree<T>.Items
            => Items;
        bool IReadOnlyTree<T>.Contains(T value)
            => Contains(value);
        IReadOnlyTree<Target> IReadOnlyTree<T>.Translate<Target>(Func<T, Target> selector, Func<Target, bool> guard)
            => Translate(selector, guard);
        IEnumerable<IReadOnlyTree<T>> IReadOnlyTree<T>.AllItems_ParentsFirst
            => AllItems_ParentsFirst;
        IEnumerable<IReadOnlyTree<T>> IReadOnlyTree<T>.AllItems_ChildrenFirst
            => AllItems_ChildrenFirst;
        IEnumerable<IReadOnlyTree<T>> IReadOnlyTree<T>.AllItems_FromTheTop
            => AllItems_FromTheTop;
        IEnumerable<IReadOnlyTree<T>> IReadOnlyTree<T>.AllItems_FromTheBottom
            => AllItems_FromTheBottom;
        #endregion
    }

    public sealed class TreeHolder<T>
    {
        public Tree<T> Tree { get; set; }
    }
}
