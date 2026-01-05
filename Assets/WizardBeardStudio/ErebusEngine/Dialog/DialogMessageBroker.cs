using System;
using UnityEngine;
using WizardBeardStudio.ErebusEngine.Core;
using WizardBeardStudio.ErebusEngine.EventBus;
using WizardBeardStudio.Events.Dialog;

namespace WizardBeardStudio.ErebusEngine.Dialog
{
    [RequireComponent(typeof(SharedEventBus))]
    public class DialogMessageBroker : Singleton<DialogMessageBroker>
    {
        private static SharedEventBus _sharedEventBus;
        
        [field: SerializeField] public DialogManager[] DialogManagers { get; private set; }

        private void OnEnable()
        {
            _sharedEventBus = SharedEventBus.Instance;
            _sharedEventBus.Subscribe<StartDialogEvent>(OnStartDialogEvent);
            _sharedEventBus.Subscribe<EndDialogEvent>(OnEndDialogEvent);
        }

        private void OnDisable()
        {
            _sharedEventBus.Unsubscribe<StartDialogEvent>(OnStartDialogEvent);
            _sharedEventBus.Unsubscribe<EndDialogEvent>(OnEndDialogEvent);
        }

        private void Start()
        {
            // _sharedEventBus = SharedEventBus.Instance;
            // _sharedEventBus.Subscribe<StartDialogEvent>(OnStartDialogEvent);
            // _sharedEventBus.Subscribe<EndDialogEvent>(OnEndDialogEvent);
            
            if (DialogManagers is { Length: > 0 }) return;
            DialogManagers = FindObjectsByType<DialogManager>(FindObjectsSortMode.None);
        }

        private void OnStartDialogEvent(StartDialogEvent e)
        {
            if (DialogManagers is not { Length: > 0 }) return;
            foreach (var dialogManager in DialogManagers)
            {
                if (dialogManager.name == e.DialogManagerName)
                {
                    dialogManager.StartDialog();
                }
            }
        }

        private void OnEndDialogEvent(EndDialogEvent e)
        {
            if (DialogManagers is not { Length: > 0 }) return;
            foreach (var dialogManager in DialogManagers)
            {
                if (dialogManager.name == e.DialogManagerName)
                {
                    dialogManager.EndDialog();
                }
            }
        }
    }
}