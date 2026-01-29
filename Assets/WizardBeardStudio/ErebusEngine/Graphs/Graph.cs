using System;
using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.Graphs
{
    /// <summary>
    /// Generic directed graph with adjacency lists.
    /// Node key: TNode
    /// Edge payload: TEdge (optional metadata)
    /// Weight: float (optional, default 1)
    /// </summary>
    public sealed class Graph<TNode, TEdge>
        where TNode : notnull
    {
        public sealed class Edge
        {
            public TNode To { get; }
            public float Weight { get; }
            public TEdge Data { get; }

            public Edge(TNode to, float weight, TEdge data)
            {
                To = to;
                Weight = weight;
                Data = data;
            }

            public override string ToString() => $"-> {To} (w={Weight})";
        }

        private readonly Dictionary<TNode, List<Edge>> _adj = new();

        public IEnumerable<TNode> Nodes => _adj.Keys;

        public bool ContainsNode(TNode node) => _adj.ContainsKey(node);

        public void AddNode(TNode node)
        {
            if (!_adj.ContainsKey(node))
                _adj[node] = new List<Edge>();
        }

        public bool RemoveNode(TNode node)
        {
            if (!_adj.Remove(node))
                return false;

            // Remove incoming edges to this node
            foreach (var kv in _adj)
            {
                kv.Value.RemoveAll(e => EqualityComparer<TNode>.Default.Equals(e.To, node));
            }
            return true;
        }

        public void AddEdge(TNode from, TNode to, float weight = 1f, TEdge data = default!)
        {
            AddNode(from);
            AddNode(to);
            _adj[from].Add(new Edge(to, weight, data));
        }

        public bool RemoveEdge(TNode from, TNode to, Predicate<Edge>? match = null)
        {
            if (!_adj.TryGetValue(from, out var list)) return false;

            int removed = list.RemoveAll(e =>
                EqualityComparer<TNode>.Default.Equals(e.To, to) &&
                (match == null || match(e)));

            return removed > 0;
        }

        public IReadOnlyList<Edge> OutEdges(TNode from)
        {
            if (!_adj.TryGetValue(from, out var list))
                return Array.Empty<Edge>();
            return list;
        }

        public int OutDegree(TNode node) => _adj.TryGetValue(node, out var list) ? list.Count : 0;

        public IEnumerable<TNode> Neighbors(TNode from)
        {
            if (!_adj.TryGetValue(from, out var list))
                yield break;

            foreach (var e in list)
                yield return e.To;
        }

        /// <summary>
        /// Convenience helper for "quest graphs" that often want reverse edges.
        /// </summary>
        public Graph<TNode, TEdge> Reverse()
        {
            var g = new Graph<TNode, TEdge>();
            foreach (var n in Nodes) g.AddNode(n);

            foreach (var from in Nodes)
            {
                foreach (var e in OutEdges(from))
                {
                    g.AddEdge(e.To, from, e.Weight, e.Data);
                }
            }
            return g;
        }
    }
}
