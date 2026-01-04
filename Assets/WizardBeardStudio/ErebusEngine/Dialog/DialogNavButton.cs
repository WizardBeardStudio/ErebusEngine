using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WizardBeardStudio.ErebusEngine.Core;

namespace WizardBeardStudio.ErebusEngine.Dialog
{
    /// <summary>
    /// Simple <see cref="GameObject"/> dynamically managed by <see cref="DialogManager"/>.
    /// </summary>
    public class DialogNavButton : MonoBehaviour
    {
        [Header("Hierarchy")]
        [Tooltip("This Button should be a child GameObject in the Hierarchy.")]
        [field: SerializeField] public Button NavButton { get; private set; }
        
        [Tooltip("This TextMeshPro Text component should be a child GameObject to a Button in the Hierarchy.")]
        [field: SerializeField] public TMP_Text Label { get; private set; }

        private DialogManager _dialogManager;
        private GameObjectTree<DialogPage> _targetNode;
        private bool _isBack;

        /// <summary>
        /// Public interface to configure a dynamically created <see cref="DialogNavButton"/> component.
        /// </summary>
        /// <param name="text"><see cref="string"/> text for the Label.</param>
        /// <param name="dialogManager"><see cref="DialogManager"/> instance used in the OnClicked() handler.</param>
        /// <param name="targetNode"><see cref="GameObjectTree{T}"/> where T : <see cref="DialogPage"/></param>
        /// <param name="isBack"><see cref="bool"/> switch to determine if this is a "Back" Button.</param>
        public void Initialize(
            string text, 
            DialogManager dialogManager, 
            GameObjectTree<DialogPage> targetNode,
            bool isBack = false)
        {
            _dialogManager = dialogManager;
            _targetNode = targetNode;

            if (_dialogManager == null || _targetNode == null) return;
            
            _isBack = isBack;
            
            if (Label != null)
            {
                Label.text = text;
            }

            if (NavButton == null)
            {
                NavButton = GetComponent<Button>();
            }
            
            NavButton.onClick.RemoveAllListeners();
            NavButton.onClick.AddListener(OnClicked);
        }

        /// <summary>
        /// Event handler triggered when a <see cref="Button"/> is clicked.
        /// </summary>
        private void OnClicked()
        {
            Debug.Log($"[Dialog Nav Button] _targetNode.Value.gameObject.name={_targetNode.Value.gameObject.name}");
            
            if (_dialogManager == null) return;

            if (_isBack)
            {
                _dialogManager.GoBack();
            }
            else if (_targetNode != null)
            {
                _dialogManager.GoToNode(_targetNode);
            }
        }
    }
}