using TMPro;
using UnityEngine;
using WizardBeardStudio.ErebusEngine.Core;
using WizardBeardStudio.ErebusEngine.EventBus;
using WizardBeardStudio.ErebusEngine.PlayerCharacter;
using WizardBeardStudio.Events.Dialog;
using WizardBeardStudio.Events.PlayerCharacter;

namespace WizardBeardStudio.ErebusEngine.UI
{
    public class HUDManager : Singleton<HUDManager>
    {
        private static SharedEventBus _sharedEventBus;
        private static ExperienceManager _experienceManager;
        private static Player _player;
        
        [Header("XP Management Demo")]
        [field: SerializeField] public int XpToAdd { get; private set; }
        [field: SerializeField] public int XpMultiplier { get; private set; }
        [SerializeField] private TMP_Text currentXp;
        [SerializeField] private TMP_Text targetXp;
        [SerializeField] private TMP_Text level;
        
        [Header("Dialog System Demo")]
        [Tooltip("The Dialog Manager Name should match the name of the GameObject with the DialogManager component.")]
        [field: SerializeField] public string DialogManagerName { get; private set; }

        private void Start()
        {
            _sharedEventBus = SharedEventBus.Instance;
            _experienceManager = ExperienceManager.Instance;
            _player = Player.Instance;
        }

        private void Update()
        {
            currentXp.text = $"XP: {_experienceManager.CurrentXp.ToString()}";
            targetXp.text = $"Next: {_experienceManager.TargetXp.ToString()}";
            level.text = $"Level: {_player.Level.ToString()}";
        }

        public void AddXp()
        {
            var gainXpEvent = new GainExperienceEvent(XpToAdd, XpMultiplier);
            _sharedEventBus.Publish(gainXpEvent);
            Debug.Log($"[HUD Manager] Adding GainExperienceEvent: Total={gainXpEvent.Total.ToString()}, Multiplier={gainXpEvent.Multiplier.ToString()}");
        }

        public void StartSimpleDialog()
        {
            if (string.IsNullOrEmpty(DialogManagerName)) return;
            var startDialogEvent = new StartDialogEvent(DialogManagerName);
            _sharedEventBus.Publish(startDialogEvent);
            Debug.Log($"[HUD Manager] Adding StartDialogEvent: DialogManagerName={DialogManagerName}");
        }

        public void EndSimpleDialog()
        {
            if (string.IsNullOrEmpty(DialogManagerName)) return;
            var endDialogEvent = new EndDialogEvent(DialogManagerName);
            _sharedEventBus.Publish(endDialogEvent);
            Debug.Log($"[HUD Manager] Adding EndDialogEvent: DialogManagerName={DialogManagerName}");
        }
    }
}