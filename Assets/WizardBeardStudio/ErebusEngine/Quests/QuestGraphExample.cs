using System;
using System.Collections.Generic;
using UnityEngine;
using WizardBeardStudio.ErebusEngine.Graphs;

namespace WizardBeardStudio.ErebusEngine.Quests
{
    public sealed class QuestGraphExample : MonoBehaviour
    {
        [Header("Quest content")]
        [SerializeField] private List<QuestStep> steps = new();

        private Graph<string, QuestTransition> _graph;
        private readonly HashSet<string> _playerTags = new();

        private void Awake()
        {
            BuildExampleGraph();
        }

        private void BuildExampleGraph()
        {
            _graph = new Graph<string, QuestTransition>();

            // Nodes
            foreach (var s in steps)
            {
                if (string.IsNullOrEmpty(s.Id)) continue;
                _graph.AddNode(s.Id);
            }

            // Example branching:
            // start -> talk
            // talk -> fight (requires "hasSword")
            // talk -> sneak (requires "hasCloak")
            // fight -> end
            // sneak -> end

            _graph.AddEdge("start", "talk", 1f, new QuestTransition { Label = "Approach the village", RequiresTags = new List<string>() });
            _graph.AddEdge("talk", "fight", 1f, new QuestTransition { Label = "Challenge the guard", RequiresTags = new List<string> { "hasSword" } });
            _graph.AddEdge("talk", "sneak", 1f, new QuestTransition { Label = "Slip past quietly", RequiresTags = new List<string> { "hasCloak" } });
            _graph.AddEdge("fight", "end", 1f, new QuestTransition { Label = "Claim victory", RequiresTags = new List<string>() });
            _graph.AddEdge("sneak", "end", 1f, new QuestTransition { Label = "Reach the relic", RequiresTags = new List<string>() });

            Validate();
        }

        private void Validate()
        {
            // Quest graphs are typically intended as DAGs.
            bool hasCycle = GraphAlgorithms.HasCycleDirected(_graph);
            if (hasCycle)
                Debug.LogWarning("[QuestGraph] Cycle detected. This may be intentional, or it may break topological reasoning.");

            if (GraphAlgorithms.TryTopologicalSort(_graph, out var order))
                Debug.Log($"[QuestGraph] Topological order: {string.Join(" -> ", order)}");
        }

        // Example gating: only edges whose requirements are met
        private bool CanVisitNode(string nodeId) => true;

        private bool EdgeIsAvailable(QuestTransition t)
        {
            if (t == null) return true;
            if (t.RequiresTags == null || t.RequiresTags.Count == 0) return true;

            foreach (var tag in t.RequiresTags)
            {
                if (!_playerTags.Contains(tag))
                    return false;
            }
            return true;
        }

        public List<(string to, QuestTransition data)> GetAvailableChoices(string fromId)
        {
            var result = new List<(string to, QuestTransition data)>();

            foreach (var e in _graph.OutEdges(fromId))
            {
                if (!CanVisitNode(e.To)) continue;
                if (!EdgeIsAvailable(e.Data)) continue;

                result.Add((e.To, e.Data));
            }

            return result;
        }

        public bool TryFindShortestPathTo(string start, string goal, out List<string> path, out float cost)
        {
            // Weighted shortest path (Dijkstra)
            return GraphAlgorithms.TryShortestPathDijkstra(_graph, start, goal, out path, out cost, canVisit: CanVisitNode);
        }

        public IEnumerable<string> DebugReachableBfs(string start)
            => GraphAlgorithms.BreadthFirst(_graph, start, canVisit: CanVisitNode);

        public IEnumerable<string> DebugReachableDfs(string start)
            => GraphAlgorithms.DepthFirst(_graph, start, canVisit: CanVisitNode);
    }
}
