using System;
using System.Collections.Generic;

namespace WizardBeardStudio.ErebusEngine.Graphs
{
    public static class GraphAlgorithms
    {
        // -------------------------
        // BFS / DFS
        // -------------------------

        public static IEnumerable<TNode> BreadthFirst<TNode, TEdge>(
            Graph<TNode, TEdge> g,
            TNode start,
            Func<TNode, bool>? canVisit = null)
            where TNode : notnull
        {
            var visited = new HashSet<TNode>();
            var q = new Queue<TNode>();

            if (canVisit != null && !canVisit(start))
                yield break;

            visited.Add(start);
            q.Enqueue(start);

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                yield return cur;

                foreach (var e in g.OutEdges(cur))
                {
                    var nxt = e.To;
                    if (visited.Contains(nxt)) continue;
                    if (canVisit != null && !canVisit(nxt)) continue;

                    visited.Add(nxt);
                    q.Enqueue(nxt);
                }
            }
        }

        public static IEnumerable<TNode> DepthFirst<TNode, TEdge>(
            Graph<TNode, TEdge> g,
            TNode start,
            Func<TNode, bool>? canVisit = null)
            where TNode : notnull
        {
            var visited = new HashSet<TNode>();
            var stack = new Stack<TNode>();

            if (canVisit != null && !canVisit(start))
                yield break;

            stack.Push(start);

            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                if (visited.Contains(cur)) continue;
                if (canVisit != null && !canVisit(cur)) continue;

                visited.Add(cur);
                yield return cur;

                // Reverse push yields a more stable "in-order" feel if edges were added in-order
                var edges = g.OutEdges(cur);
                for (int i = edges.Count - 1; i >= 0; i--)
                {
                    var nxt = edges[i].To;
                    if (!visited.Contains(nxt))
                        stack.Push(nxt);
                }
            }
        }

        // -------------------------
        // BFS shortest path (unweighted)
        // -------------------------

        public static bool TryShortestPathUnweighted<TNode, TEdge>(
            Graph<TNode, TEdge> g,
            TNode start,
            TNode goal,
            out List<TNode> path,
            Func<TNode, bool>? canVisit = null)
            where TNode : notnull
        {
            path = new List<TNode>();
            var parent = new Dictionary<TNode, TNode>();
            var visited = new HashSet<TNode>();
            var q = new Queue<TNode>();

            if (canVisit != null && (!canVisit(start) || !canVisit(goal)))
                return false;

            visited.Add(start);
            q.Enqueue(start);

            while (q.Count > 0)
            {
                var cur = q.Dequeue();
                if (EqualityComparer<TNode>.Default.Equals(cur, goal))
                {
                    path = ReconstructPath(start, goal, parent);
                    return true;
                }

                foreach (var e in g.OutEdges(cur))
                {
                    var nxt = e.To;
                    if (visited.Contains(nxt)) continue;
                    if (canVisit != null && !canVisit(nxt)) continue;

                    visited.Add(nxt);
                    parent[nxt] = cur;
                    q.Enqueue(nxt);
                }
            }

            return false;
        }

        // -------------------------
        // Dijkstra shortest path (weighted, non-negative)
        // -------------------------

        public static bool TryShortestPathDijkstra<TNode, TEdge>(
            Graph<TNode, TEdge> g,
            TNode start,
            TNode goal,
            out List<TNode> path,
            out float totalCost,
            Func<TNode, bool>? canVisit = null)
            where TNode : notnull
        {
            path = new List<TNode>();
            totalCost = float.PositiveInfinity;

            if (canVisit != null && (!canVisit(start) || !canVisit(goal)))
                return false;

            var dist = new Dictionary<TNode, float>();
            var parent = new Dictionary<TNode, TNode>();
            var pq = new MinPriorityQueue<TNode>();

            dist[start] = 0f;
            pq.Push(start, 0f);

            while (pq.Count > 0)
            {
                var cur = pq.Pop(out var curPri);

                // Stale entry check
                if (!dist.TryGetValue(cur, out var best) || curPri > best)
                    continue;

                if (EqualityComparer<TNode>.Default.Equals(cur, goal))
                {
                    totalCost = best;
                    path = ReconstructPath(start, goal, parent);
                    return true;
                }

                foreach (var e in g.OutEdges(cur))
                {
                    var nxt = e.To;
                    if (canVisit != null && !canVisit(nxt)) continue;

                    if (e.Weight < 0f)
                        throw new InvalidOperationException("Dijkstra requires non-negative edge weights.");

                    var cand = best + e.Weight;

                    if (!dist.TryGetValue(nxt, out var old) || cand < old)
                    {
                        dist[nxt] = cand;
                        parent[nxt] = cur;
                        pq.Push(nxt, cand);
                    }
                }
            }

            return false;
        }

        public static Dictionary<TNode, float> DijkstraAll<TNode, TEdge>(
            Graph<TNode, TEdge> g,
            TNode start,
            Func<TNode, bool>? canVisit = null)
            where TNode : notnull
        {
            var dist = new Dictionary<TNode, float>();
            var pq = new MinPriorityQueue<TNode>();

            if (canVisit != null && !canVisit(start))
                return dist;

            dist[start] = 0f;
            pq.Push(start, 0f);

            while (pq.Count > 0)
            {
                var cur = pq.Pop(out var curPri);

                if (!dist.TryGetValue(cur, out var best) || curPri > best)
                    continue;

                foreach (var e in g.OutEdges(cur))
                {
                    var nxt = e.To;
                    if (canVisit != null && !canVisit(nxt)) continue;

                    if (e.Weight < 0f)
                        throw new InvalidOperationException("Dijkstra requires non-negative edge weights.");

                    var cand = best + e.Weight;

                    if (!dist.TryGetValue(nxt, out var old) || cand < old)
                    {
                        dist[nxt] = cand;
                        pq.Push(nxt, cand);
                    }
                }
            }

            return dist;
        }

        // -------------------------
        // Cycle detection (directed)
        // -------------------------

        public static bool HasCycleDirected<TNode, TEdge>(Graph<TNode, TEdge> g)
            where TNode : notnull
        {
            var state = new Dictionary<TNode, int>(); // 0=unseen, 1=visiting, 2=done

            foreach (var n in g.Nodes)
            {
                if (!state.TryGetValue(n, out var s)) s = 0;
                if (s == 0 && DfsCycle(n))
                    return true;
            }

            return false;

            bool DfsCycle(TNode u)
            {
                state[u] = 1;

                foreach (var e in g.OutEdges(u))
                {
                    var v = e.To;
                    if (!state.TryGetValue(v, out var sv)) sv = 0;

                    if (sv == 1) return true;
                    if (sv == 0 && DfsCycle(v)) return true;
                }

                state[u] = 2;
                return false;
            }
        }

        // -------------------------
        // Topological sort (directed, DAG)
        // -------------------------

        public static bool TryTopologicalSort<TNode, TEdge>(
            Graph<TNode, TEdge> g,
            out List<TNode> order)
            where TNode : notnull
        {
            order = new List<TNode>();

            // Kahn’s algorithm
            var indeg = new Dictionary<TNode, int>();
            foreach (var n in g.Nodes)
                indeg[n] = 0;

            foreach (var u in g.Nodes)
            {
                foreach (var e in g.OutEdges(u))
                {
                    if (!indeg.ContainsKey(e.To))
                        indeg[e.To] = 0;
                    indeg[e.To] += 1;
                }
            }

            var q = new Queue<TNode>();
            foreach (var kv in indeg)
            {
                if (kv.Value == 0)
                    q.Enqueue(kv.Key);
            }

            while (q.Count > 0)
            {
                var u = q.Dequeue();
                order.Add(u);

                foreach (var e in g.OutEdges(u))
                {
                    var v = e.To;
                    indeg[v] -= 1;
                    if (indeg[v] == 0)
                        q.Enqueue(v);
                }
            }

            // If not all nodes were output, there is a cycle
            return order.Count == indeg.Count;
        }

        // -------------------------
        // Strongly Connected Components (Tarjan)
        // -------------------------

        public static List<List<TNode>> StronglyConnectedComponents<TNode, TEdge>(Graph<TNode, TEdge> g)
            where TNode : notnull
        {
            int index = 0;
            var stack = new Stack<TNode>();
            var onStack = new HashSet<TNode>();
            var idx = new Dictionary<TNode, int>();
            var low = new Dictionary<TNode, int>();
            var result = new List<List<TNode>>();

            foreach (var v in g.Nodes)
            {
                if (!idx.ContainsKey(v))
                    StrongConnect(v);
            }

            return result;

            void StrongConnect(TNode v)
            {
                idx[v] = index;
                low[v] = index;
                index++;

                stack.Push(v);
                onStack.Add(v);

                foreach (var e in g.OutEdges(v))
                {
                    var w = e.To;

                    if (!idx.ContainsKey(w))
                    {
                        StrongConnect(w);
                        low[v] = Math.Min(low[v], low[w]);
                    }
                    else if (onStack.Contains(w))
                    {
                        low[v] = Math.Min(low[v], idx[w]);
                    }
                }

                if (low[v] == idx[v])
                {
                    var comp = new List<TNode>();
                    while (true)
                    {
                        var w = stack.Pop();
                        onStack.Remove(w);
                        comp.Add(w);
                        if (EqualityComparer<TNode>.Default.Equals(w, v))
                            break;
                    }
                    result.Add(comp);
                }
            }
        }

        // -------------------------
        // Helpers
        // -------------------------

        private static List<TNode> ReconstructPath<TNode>(
            TNode start,
            TNode goal,
            Dictionary<TNode, TNode> parent)
            where TNode : notnull
        {
            var path = new List<TNode>();
            var cur = goal;
            path.Add(cur);

            while (!EqualityComparer<TNode>.Default.Equals(cur, start))
            {
                if (!parent.TryGetValue(cur, out var p))
                    return new List<TNode>(); // unreachable or inconsistent

                cur = p;
                path.Add(cur);
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Minimal priority queue (binary heap) to avoid external deps.
        /// </summary>
        private sealed class MinPriorityQueue<T>
            where T : notnull
        {
            private readonly List<(T item, float pri)> _heap = new();

            public int Count => _heap.Count;

            public void Push(T item, float pri)
            {
                _heap.Add((item, pri));
                SiftUp(_heap.Count - 1);
            }

            public T Pop(out float pri)
            {
                if (_heap.Count == 0) throw new InvalidOperationException("Empty queue.");

                var root = _heap[0];
                pri = root.pri;

                var last = _heap[_heap.Count - 1];
                _heap.RemoveAt(_heap.Count - 1);

                if (_heap.Count > 0)
                {
                    _heap[0] = last;
                    SiftDown(0);
                }

                return root.item;
            }

            private void SiftUp(int i)
            {
                while (i > 0)
                {
                    int p = (i - 1) / 2;
                    if (_heap[p].pri <= _heap[i].pri) break;
                    (_heap[p], _heap[i]) = (_heap[i], _heap[p]);
                    i = p;
                }
            }

            private void SiftDown(int i)
            {
                while (true)
                {
                    int l = i * 2 + 1;
                    int r = i * 2 + 2;
                    int smallest = i;

                    if (l < _heap.Count && _heap[l].pri < _heap[smallest].pri) smallest = l;
                    if (r < _heap.Count && _heap[r].pri < _heap[smallest].pri) smallest = r;

                    if (smallest == i) break;

                    (_heap[i], _heap[smallest]) = (_heap[smallest], _heap[i]);
                    i = smallest;
                }
            }
        }
    }
}
