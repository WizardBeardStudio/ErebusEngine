using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WizardBeardStudio.ErebusEngine.Core;

namespace WizardBeardStudio.ErebusEngine.Dialog
{
    public class DialogManager : MonoBehaviour
    {
        [Header("Hierarchy")]
        [Tooltip("Root Transform containing all DialogPage GameObjects. Typically a GameObject named 'Pages'.")]
        [SerializeField] private Transform pagesRoot;

        [Header("UI")] 
        // [field: SerializeField] public TMP_Text TitleText { get; private set; }
        // [field: SerializeField] public TMP_Text DialogText { get; private set; }
        // [field: SerializeField] public Sprite Portrait { get; private set; }
        [field: SerializeField] public ScrollRect ScrollableRect { get; private set; }
        
        [Tooltip("Parent Transform to hold dynamically created navigation buttons.")]
        [field: SerializeField] public Transform NavButtonContainer { get; private set; }
        
        [Tooltip("Prefab of navigation button of type DialogNavButton.")]
        [field: SerializeField] public DialogNavButton NavButtonPrefab { get; private set; }
        
        /// <summary>
        /// Roots of the dialog trees (one per top-level DialogPage under pagesRoot).
        /// </summary>
        public IReadOnlyList<GameObjectTree<DialogPage>> Roots => _roots.AsReadOnly();

        /// <summary>
        /// Entry node to start dialog flow from.
        /// </summary>
        public GameObjectTree<DialogPage> EntryNode { get; private set; }

        private readonly List<GameObjectTree<DialogPage>> _roots = new();
        private readonly Dictionary<DialogPage, GameObjectTree<DialogPage>> _nodeByPage = new();
        private readonly Stack<GameObjectTree<DialogPage>> _history = new();
        
        private GameObjectTree<DialogPage> _currentNode;
        
        private void Awake()
        {
            EnsurePagesRoot();
            if (pagesRoot == null)
            {
                Debug.LogError("[Dialog Manager] pagesRoot not set and no GameObject named 'Pages' found.");
                return;
            }

            BuildForestFromHierarchy();
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

        private void BuildForestFromHierarchy()
        {
            _roots.Clear();
            _nodeByPage.Clear();

            // Find all DialogPage components under pagesRoot (including inactive)
            var pages = pagesRoot.GetComponentsInChildren<DialogPage>(includeInactive: true);

            // Create nodes
            foreach (var page in pages)
            {
                if (page == null) continue;
                var node = new GameObjectTree<DialogPage>(page);
                _nodeByPage[page] = node;
            }

            // Wire up parent/child by walking the Transform hierarchy
            foreach (var page in pages)
            {
                if (page == null) continue;

                var node = _nodeByPage[page];
                var parentTransform = page.transform.parent;

                GameObjectTree<DialogPage> parentNode = null;

                // Walk up until we hit another DialogPage OR the pagesRoot
                while (parentTransform != null && parentTransform != pagesRoot)
                {
                    var parentPage = parentTransform.GetComponent<DialogPage>();
                    if (parentPage != null && _nodeByPage.TryGetValue(parentPage, out var foundParentNode))
                    {
                        parentNode = foundParentNode;
                        break;
                    }

                    parentTransform = parentTransform.parent;
                }

                if (parentNode != null)
                {
                    parentNode.AddChild(node);
                }
                else
                {
                    // No DialogPage ancestor under pagesRoot, so this is a root
                    _roots.Add(node);
                }
            }

            Debug.Log($"[Dialog Manager] Built dialog forest. Root count: {_roots.Count}");
        }

        private void ChooseEntryNode()
        {
            // Prefer an explicitly marked start page
            foreach (var kvp in _nodeByPage)
            {
                if (kvp.Key != null && kvp.Key.IsStartPage)
                {
                    EntryNode = kvp.Value;
                    break;
                }
            }

            // Fallback: first root
            if (EntryNode == null && _roots.Count > 0)
            {
                EntryNode = _roots[0];
            }

            if (EntryNode != null)
            {
                Debug.Log($"[Dialog Manager] Entry node set to: {EntryNode.Value.name} ({EntryNode.Value.Title})");
            }
            else
            {
                Debug.LogWarning("[Dialog Manager] No EntryNode determined; dialog forest is empty.");
            }
        }
        
        public void StartDialog()
        {
            if (EntryNode == null)
            {
                Debug.LogWarning("[Dialog Manager] Cannot start dialog, EntryNode is null.");
                return;
            }
            GoToNode(EntryNode, true);
        }

        private void ShowPage(GameObjectTree<DialogPage> node)
        {
            if (node == null || node.Value == null)
            {
                Debug.LogWarning("[Dialog Manager] Attempted to show a null node or page.");
                return;
            }

            var page = node.Value;

            // Drive UI from the page’s data
            Debug.Log($"[Dialog Manager] Showing page '{page.Title}' (GameObject: {page.name})");
            Debug.Log(page.DialogText);
            
            // TODO: Send an Event, and have the Pages listen for events to show, populate data, etc.?
            page.gameObject.SetActive(true);
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
            
            GoToNode(node: previous, addToHistory: false);
        }

        private void RenderCurrentPage()
        {
            if (_currentNode == null || _currentNode.Value == null)
            {
                Debug.LogWarning($"[Dialog Manager] RenderCurrentPage() called where null == _currentNode || _currentNode.Value");
                return;
            }
            
            RebuildNavButtons();
        }

        private void RebuildNavButtons()
        {
            if (NavButtonContainer == null || NavButtonPrefab == null)
            {
                Debug.LogWarning($"[Dialog Manager] NavButton || NavButtonPrefab == null.");
                return;
            }

            for (int i = NavButtonContainer.childCount - 1; i >= 0; i--)
            {
                var child = NavButtonContainer.GetChild(i);
                Destroy(child.gameObject);
            }

            if (_history.Count > 0)
            {
                var previousNode = _history.Peek();

                var backButton = Instantiate(NavButtonPrefab, NavButtonContainer);
                backButton.Initialize("Previous", this, previousNode, isBack: true);
            }

            if (_currentNode.Parent != null)
            {
                foreach (var sibling in _currentNode.Siblings())
                {
                    if (sibling == null || sibling.Value == null) continue;

                    var button = Instantiate(NavButtonPrefab, NavButtonContainer);
                    var label = string.IsNullOrEmpty(sibling.Value.Title)
                        ? sibling.Value.name
                        : sibling.Value.Title;
                    
                    button.Initialize(label, this, sibling);
                }
            }

            foreach (var child in _currentNode.Children)
            {
                if (child == null) continue;

                var button = Instantiate(NavButtonPrefab, NavButtonContainer);
                var label = string.IsNullOrEmpty(child.Value.Title)
                    ? child.Value.name
                    : child.Value.Title;
                
                button.Initialize(label, this, child);
            }
        }
        
        public IEnumerable<GameObjectTree<DialogPage>> DepthFirstAll()
        {
            foreach (var root in _roots)
            {
                foreach (var node in GameObjectTree<DialogPage>.DepthFirst(root))
                {
                    yield return node;
                }
            }
        }

        public IEnumerable<GameObjectTree<DialogPage>> BreadthFirstAll()
        {
            foreach (var root in _roots)
            {
                foreach (var node in GameObjectTree<DialogPage>.BreadthFirst(root))
                {
                    yield return node;
                }
            }
        }
    }
}
