using System;
using System.Collections.Generic;
using UnityEngine;

namespace WizardBeardStudio.ErebusEngine.Core
{
    public class GameObjectTree<T> where T : MonoBehaviour
    {
        public T Value { get; private set; }
        public GameObjectTree<T> Parent { get; set; }
        public IReadOnlyList<GameObjectTree<T>> Children => _children.AsReadOnly();

        private readonly List<GameObjectTree<T>> _children = new();

        public GameObjectTree (T value)
        {
            Value = value;
        }

        internal void SetParent(GameObjectTree<T> parent)
        {
            Parent = parent;
            parent._children.Add(this);
        }
        
        public GameObjectTree<T> AddChild(GameObjectTree<T> child)
        {
            if (child == null) throw new ArgumentNullException();
            if (child.Parent != null) throw new InvalidOperationException("Node already has a parent Node.");
            
            _children.Add(child);
            child.Parent = this;

            return this;
        }

        public bool RemoveChild(GameObjectTree<T> child)
        {
            if (_children.Remove(child))
            {
                child.Parent = null;
                return true;
            }

            return false;
        }

        public IEnumerable<GameObjectTree<T>> Siblings()
        {
            if (Parent == null) yield break;
            foreach (var sibling in Parent.Children)
            {
                if (!ReferenceEquals(sibling, this))
                {
                    yield return sibling;
                }
            }
        }

        public GameObjectTree<T> GetChild(int index)
        {
            return (index >= 0 && index < _children.Count)
                ? _children[index]
                : null;
        }

        public GameObjectTree<T> NextSibling()
        {
            if (Parent == null) return null;

            var siblings = Parent._children;
            var index = siblings.IndexOf(this);
            if (index < 0 || index + 1 >= siblings.Count) return null;

            return siblings[index + 1];
        }

        public GameObjectTree<T> PreviousSibling()
        {
            if (Parent == null) return null;

            var siblings = Parent._children;
            var index = siblings.IndexOf(this);
            if (index <= 0) return null;

            return siblings[index - 1];
        }

        public override string ToString() => Value?.ToString() ?? "<null>";

        public static IEnumerable<GameObjectTree<T>> DepthFirst(GameObjectTree<T> root)
        {
            var stack = new Stack<GameObjectTree<T>>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                var node = stack.Pop();
                yield return node;

                for (int i = node.Children.Count - 1; i >= 0; i--)
                {
                    stack.Push(node.Children[i]);
                }
            }
        }

        public static IEnumerable<GameObjectTree<T>> BreadthFirst(GameObjectTree<T> root)
        {
            var queue = new Queue<GameObjectTree<T>>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                yield return node;

                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        public static IEnumerable<GameObjectTree<T>> PathToRoot(GameObjectTree<T> node)
        {
            while (node != null)
            {
                yield return node;
                node = node.Parent!;
            }
        }
    }
}
