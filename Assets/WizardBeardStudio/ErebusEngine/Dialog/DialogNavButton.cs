using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WizardBeardStudio.ErebusEngine.Core;

namespace WizardBeardStudio.ErebusEngine.Dialog
{
    public class DialogNavButton : MonoBehaviour
    {
        [field: SerializeField] public Button NavButton { get; private set; }
        [field: SerializeField] public TMP_Text Label { get; private set; }

        private DialogManager _dialogManager;
        private GameObjectTree<DialogPage> _targetNode;
        private bool _isBack;

        public void Initialize(
            string text, 
            DialogManager dialogManager, 
            GameObjectTree<DialogPage> targetNode,
            bool isBack = false)
        {
            _dialogManager = dialogManager;
            _targetNode = targetNode;
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

        private void OnClicked()
        {
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