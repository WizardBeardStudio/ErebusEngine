using System.Collections.Generic;
using UnityEngine;
using WizardBeardStudio.ErebusEngine.Core;

namespace WizardBeardStudio.ErebusEngine.Dialog
{
    public class DialogManager : MonoBehaviour
    {
        [Header("Hierarchy")]
        [Tooltip("Root Transform containing all DialogPage GameObjects. Typically a GameObject named 'Pages'.")]
        [SerializeField] private Transform pagesRoot;

        [Tooltip("DialogPage component that lives on pagesRoot. This is the synthetic root node.")]
        [SerializeField] private DialogPage syntheticRootPage;

        // Source of truth: single tree root
        private GameObjectTree<DialogPage> _gameObjectTree;

        // Lookup for building / navigation
        private readonly Dictionary<DialogPage, GameObjectTree<DialogPage>> _nodeByPage = new();

        private readonly Stack<GameObjectTree<DialogPage>> _history = new();
        private GameObjectTree<DialogPage> _currentNode;

        public GameObjectTree<DialogPage> DialogGameObjectTree => _gameObjectTree; // optional public accessor
        public GameObjectTree<DialogPage> EntryNode { get; private set; }

        private void Awake()
        {
            EnsurePagesRoot();
            if (pagesRoot == null)
            {
                Debug.LogError("[Dialog Manager] pagesRoot not set and no GameObject named 'Pages' found.");
                return;
            }

            EnsureSyntheticRootPage();
            if (syntheticRootPage == null)
            {
                Debug.LogError("[Dialog Manager] syntheticRootPage not set and no DialogPage found on PagesRoot.");
                return;
            }

            BuildTreeFromHierarchy();
            ChooseEntryNode();
            StartDialog();
        }

        private void EnsurePagesRoot()
        {
            if (pagesRoot != null) return;

            var pagesObj = GameObject.Find("Pages");
            if (pagesObj != null)
            {
                pagesRoot = pagesObj.transform;
            }
        }

        private void EnsureSyntheticRootPage()
        {
            if (syntheticRootPage != null) return;

            // Preferred: designer adds DialogPage to PagesRoot in the scene.
            syntheticRootPage = pagesRoot.GetComponent<DialogPage>();

#if UNITY_EDITOR
            // If missing in editor, you can auto-add it to reduce setup friction.
            // Remove this block if you want strict authoring rules.
            if (syntheticRootPage == null)
            {
                syntheticRootPage = pagesRoot.gameObject.AddComponent<DialogPage>();
                Debug.LogWarning("[Dialog Manager] Added DialogPage to PagesRoot as synthetic root.");
            }
#endif
        }

        private void BuildTreeFromHierarchy()
        {
            _nodeByPage.Clear();

            // Get all DialogPages under PagesRoot, including the synthetic root on PagesRoot itself.
            var pages = pagesRoot.GetComponentsInChildren<DialogPage>(includeInactive: true);

            // Create nodes
            foreach (var page in pages)
            {
                if (page == null) continue;
                _nodeByPage[page] = new GameObjectTree<DialogPage>(page);
            }

            // Establish the single root
            _gameObjectTree = _nodeByPage[syntheticRootPage];

            // Wire up relationships by nearest DialogPage ancestor.
            foreach (var page in pages)
            {
                if (page == null) continue;
                if (page == syntheticRootPage) continue; // skip root

                var node = _nodeByPage[page];

                GameObjectTree<DialogPage> parentNode = FindNearestDialogPageAncestorNode(page.transform);
                if (parentNode == null)
                {
                    // No DialogPage ancestor under PagesRoot, attach to synthetic root.
                    _gameObjectTree.AddChild(node);
                }
                else
                {
                    parentNode.AddChild(node);
                }
            }

            Debug.Log($"[Dialog Manager] Built dialog tree with synthetic root: {syntheticRootPage.name}");
        }

        private GameObjectTree<DialogPage> FindNearestDialogPageAncestorNode(Transform start)
        {
            var parent = start.parent;
            while (parent != null)
            {
                // Stop when leaving the PagesRoot subtree
                if (parent == pagesRoot)
                {
                    // Parent is PagesRoot. If PagesRoot has the synthetic DialogPage, that is the nearest ancestor.
                    if (syntheticRootPage != null && parent.GetComponent<DialogPage>() == syntheticRootPage)
                    {
                        return _gameObjectTree;
                    }
                    return null;
                }

                var parentPage = parent.GetComponent<DialogPage>();
                if (parentPage != null && _nodeByPage.TryGetValue(parentPage, out var parentNode))
                {
                    return parentNode;
                }

                parent = parent.parent;
            }

            return null;
        }

        private void ChooseEntryNode()
        {
            // Prefer a page explicitly marked IsStartPage (excluding synthetic root).
            foreach (var kvp in _nodeByPage)
            {
                var page = kvp.Key;
                if (page == null) continue;
                if (page == syntheticRootPage) continue;

                if (page.IsStartPage)
                {
                    EntryNode = kvp.Value;
                    break;
                }
            }

            // Fallback: first child of synthetic root
            if (EntryNode == null && _gameObjectTree.Children.Count > 0)
            {
                EntryNode = _gameObjectTree.GetChild(0);
            }

            if (EntryNode != null)
            {
                Debug.Log($"[Dialog Manager] Entry node: {EntryNode.Value.name} ({EntryNode.Value.Title})");
            }
            else
            {
                Debug.LogWarning("[Dialog Manager] No EntryNode determined; tree is empty.");
            }
        }

        public void StartDialog()
        {
            if (EntryNode == null)
            {
                Debug.LogWarning("[Dialog Manager] Cannot start dialog; EntryNode is null.");
                return;
            }

            GoToNode(EntryNode, addToHistory: false);
        }

        public void GoToNode(GameObjectTree<DialogPage> node, bool addToHistory = true)
        {
            if (node == null) return;

            if (_currentNode != null && addToHistory)
            {
                _history.Push(_currentNode);
            }

            _currentNode = node;
            RenderCurrentPage();
            ShowPage(_currentNode);
        }

        public void GoBack()
        {
            if (_history.Count == 0) return;

            var previous = _history.Pop();
            GoToNode(previous, addToHistory: false);
        }

        private void ShowPage(GameObjectTree<DialogPage> node)
        {
            if (node == null || node.Value == null)
            {
                Debug.LogWarning("[Dialog Manager] Attempted to show a null node/page.");
                return;
            }

            var page = node.Value;
            Debug.Log($"[Dialog Manager] Showing page '{page.Title}' (GameObject: {page.name})");
            Debug.Log(page.DialogText);

            page.gameObject.SetActive(true);
        }

        private void RenderCurrentPage()
        {
            if (_currentNode == null || _currentNode.Value == null)
            {
                Debug.LogWarning("[Dialog Manager] RenderCurrentPage called with null current node/value.");
                return;
            }

            RebuildNavButtons_DemoTraversal();
        }

        /// <summary>
        /// Demonstrates traversal usage:
        /// - "Back" button from history
        /// - Sibling buttons ordered using BreadthFirst traversal from parent (filtering depth==1)
        /// - Child buttons ordered using DepthFirst traversal from current (filtering Parent==current)
        /// </summary>
        private void RebuildNavButtons_DemoTraversal()
        {
            var binder = _currentNode.Value.GetComponentInChildren<DialogPageComponentBinder>();
            if (binder == null)
            {
                Debug.LogWarning("[Dialog Manager] DialogPageComponentBinder not found under current page.");
                return;
            }

            // Clear the current page's nav container
            for (int i = binder.NavButtonContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(binder.NavButtonContainer.GetChild(i).gameObject);
            }

            // 1) Back button
            if (_history.Count > 0)
            {
                var previousNode = _history.Peek();
                var backButton = Instantiate(binder.NavButtonPrefab, binder.NavButtonContainer);
                backButton.Initialize("Previous", this, previousNode, isBack: true);
            }

            // 2) Siblings (BreadthFirst traversal from parent)
            foreach (var sib in EnumerateSiblingsBreadthFirst(_currentNode))
            {
                var label = string.IsNullOrEmpty(sib.Value.Title) ? sib.Value.name : sib.Value.Title;
                var button = Instantiate(binder.NavButtonPrefab, binder.NavButtonContainer);
                button.Initialize(label, this, sib);
            }

            // 3) Children (DepthFirst traversal from current)
            foreach (var child in EnumerateImmediateChildrenDepthFirst(_currentNode))
            {
                var label = string.IsNullOrEmpty(child.Value.Title) ? child.Value.name : child.Value.Title;
                var button = Instantiate(binder.NavButtonPrefab, binder.NavButtonContainer);
                button.Initialize(label, this, child);
            }
        }

        /// <summary>
        /// Breadth-first ordering of siblings:
        /// BFS over the parent subtree returns parent, then its direct children in order.
        /// Filter to direct children (Parent == parentNode), exclude current.
        /// </summary>
        private static IEnumerable<GameObjectTree<DialogPage>> EnumerateSiblingsBreadthFirst(GameObjectTree<DialogPage> current)
        {
            if (current == null) yield break;
            if (current.Parent == null) yield break;

            var parent = current.Parent;

            foreach (var node in GameObjectTree<DialogPage>.BreadthFirst(parent))
            {
                if (node == null) continue;

                // Siblings are the direct children of the same parent
                if (node.Parent != parent) continue;
                if (ReferenceEquals(node, current)) continue;

                yield return node;
            }
        }

        /// <summary>
        /// Depth-first ordering of immediate children:
        /// DFS over current subtree yields current then descendants.
        /// Filter to direct children (Parent == current).
        /// This demonstrates DFS usage while still returning only immediate children for nav buttons.
        /// </summary>
        private static IEnumerable<GameObjectTree<DialogPage>> EnumerateImmediateChildrenDepthFirst(GameObjectTree<DialogPage> current)
        {
            if (current == null) yield break;

            foreach (var node in GameObjectTree<DialogPage>.DepthFirst(current))
            {
                if (node == null) continue;

                // Skip self
                if (ReferenceEquals(node, current)) continue;

                // Only immediate children for navigation buttons
                if (node.Parent == current)
                {
                    yield return node;
                }
            }
        }

        // Optional reference helpers for whole-tree traversals starting at the synthetic root.
        public IEnumerable<GameObjectTree<DialogPage>> DepthFirstAll()
            => GameObjectTree<DialogPage>.DepthFirst(_gameObjectTree);

        public IEnumerable<GameObjectTree<DialogPage>> BreadthFirstAll()
            => GameObjectTree<DialogPage>.BreadthFirst(_gameObjectTree);
    }
}
