using TMPro;
using UnityEngine;
using WizardBeardStudio.ErebusEngine.Core;
using WizardBeardStudio.ErebusEngine.EventBus;
using WizardBeardStudio.ErebusEngine.PlayerCharacter;
using WizardBeardStudio.Events.PlayerCharacter;

namespace WizardBeardStudio.ErebusEngine.UI
{
    public class HUDManager : Singleton<HUDManager>
    {
        private static SharedEventBus _sharedEventBus;
        private static ExperienceManager _experienceManager;
        private static Player _player;
        
        [field: SerializeField] public int XpToAdd { get; private set; }
        [field: SerializeField] public int XpMultiplier { get; private set; }
        [SerializeField] private TMP_Text currentXp;
        [SerializeField] private TMP_Text targetXp;
        [SerializeField] private TMP_Text level;

        private void OnEnable()
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
    }
}